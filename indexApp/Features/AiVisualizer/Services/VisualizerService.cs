using indexApp.Features.AiVisualizer.Data;
using indexApp.Features.AiVisualizer.Entities;
using indexApp.Features.AiVisualizer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace indexApp.Features.AiVisualizer.Services;

public sealed class VisualizerService
{
    private readonly AiVisualizerDbContext dbContext;
    private readonly IGownVisualizationService imageGenerationService;
    private readonly MeasurementService measurementService;
    private readonly PromptBuilder promptBuilder;
    private readonly UsageLimitService usageLimitService;
    private readonly FileStorageService fileStorageService;
    private readonly AiVisualizerOptions options;

    public VisualizerService(
        AiVisualizerDbContext dbContext,
        IGownVisualizationService imageGenerationService,
        MeasurementService measurementService,
        PromptBuilder promptBuilder,
        UsageLimitService usageLimitService,
        FileStorageService fileStorageService,
        IOptions<AiVisualizerOptions> options)
    {
        this.dbContext = dbContext;
        this.imageGenerationService = imageGenerationService;
        this.measurementService = measurementService;
        this.promptBuilder = promptBuilder;
        this.usageLimitService = usageLimitService;
        this.fileStorageService = fileStorageService;
        this.options = options.Value;
    }

    public async Task<IReadOnlyList<DressStyleDto>> GetStylesAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.DressStyles
            .Where(style => style.IsActive)
            .OrderBy(style => style.Name)
            .Select(style => ToDto(style))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FabricDto>> GetFabricsAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Fabrics
            .Include(fabric => fabric.Colors.Where(color => color.IsActive))
            .Where(fabric => fabric.IsActive)
            .OrderBy(fabric => fabric.Name)
            .Select(fabric => ToDto(fabric))
            .ToListAsync(cancellationToken);
    }

    public async Task MigratePublicMediaToPrivateStorageAsync(CancellationToken cancellationToken = default)
    {
        var migratedFiles = new List<FileStorageService.LegacyMediaCopy>();
        var requests = await dbContext.VisualizerRequests
            .Where(request => request.UploadedPhotoUrl != null && request.UploadedPhotoUrl.StartsWith("/uploads/visualizer/"))
            .ToListAsync(cancellationToken);

        foreach (var request in requests)
        {
            var migrated = await fileStorageService.CopyLegacyUploadedPhotoToPrivateStorageAsync(request.UploadedPhotoUrl, cancellationToken);
            if (migrated is null)
            {
                continue;
            }

            request.UploadedPhotoUrl = migrated.StorageReference;
            migratedFiles.Add(migrated);
        }

        var previews = await dbContext.GeneratedPreviews
            .Where(preview => preview.ImageUrl.StartsWith("/uploads/visualizer/generated/"))
            .ToListAsync(cancellationToken);

        foreach (var preview in previews)
        {
            var migrated = await fileStorageService.CopyLegacyGeneratedPreviewToPrivateStorageAsync(preview.ImageUrl, cancellationToken);
            if (migrated is null)
            {
                continue;
            }

            preview.ImageUrl = migrated.StorageReference;
            migratedFiles.Add(migrated);
        }

        if (migratedFiles.Count == 0)
        {
            return;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        foreach (var migratedFile in migratedFiles)
        {
            fileStorageService.DeleteLegacyPublicFile(migratedFile.LegacyAbsolutePath);
        }
    }

    public async Task CleanupExpiredPrivateMediaAsync(CancellationToken cancellationToken = default)
    {
        var cutoff = DateTimeOffset.UtcNow.AddDays(-options.PrivateMediaRetentionDays);
        var expiredRequests = await dbContext.VisualizerRequests
            .Where(request => request.UploadedPhotoUrl != null && request.UpdatedAt <= cutoff)
            .ToListAsync(cancellationToken);
        var expiredPreviews = await dbContext.GeneratedPreviews
            .Where(preview => preview.CreatedAt <= cutoff)
            .ToListAsync(cancellationToken);

        var referencesToDelete = expiredRequests
            .Select(request => request.UploadedPhotoUrl)
            .Concat(expiredPreviews.Select(preview => preview.ImageUrl))
            .Where(reference => !string.IsNullOrWhiteSpace(reference))
            .Cast<string>()
            .ToList();

        foreach (var request in expiredRequests)
        {
            request.UploadedPhotoUrl = null;
        }

        dbContext.GeneratedPreviews.RemoveRange(expiredPreviews);
        if (expiredRequests.Count > 0 || expiredPreviews.Count > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        foreach (var reference in referencesToDelete)
        {
            fileStorageService.DeletePrivateMedia(reference);
        }

        var retainedReferences = (await dbContext.VisualizerRequests
                .Where(request => request.UploadedPhotoUrl != null)
                .Select(request => request.UploadedPhotoUrl!)
                .ToListAsync(cancellationToken))
            .Concat(await dbContext.GeneratedPreviews
                .Select(preview => preview.ImageUrl)
                .ToListAsync(cancellationToken))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        fileStorageService.DeleteUnreferencedExpiredPrivateMedia(retainedReferences, cutoff.UtcDateTime);
    }

    public async Task<bool> DeletePrivateMediaAsync(
        string userId,
        Guid requestId,
        CancellationToken cancellationToken = default)
    {
        var request = await dbContext.VisualizerRequests
            .Include(candidate => candidate.GeneratedPreviews)
            .SingleOrDefaultAsync(candidate => candidate.Id == requestId && candidate.UserId == userId, cancellationToken);
        if (request is null)
        {
            return false;
        }

        var referencesToDelete = request.GeneratedPreviews.Select(preview => preview.ImageUrl).ToList();
        if (!string.IsNullOrWhiteSpace(request.UploadedPhotoUrl))
        {
            referencesToDelete.Add(request.UploadedPhotoUrl);
        }

        request.UploadedPhotoUrl = null;
        dbContext.GeneratedPreviews.RemoveRange(request.GeneratedPreviews);
        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var reference in referencesToDelete)
        {
            fileStorageService.DeletePrivateMedia(reference);
        }

        return true;
    }

    public async Task DeleteUserSessionMediaAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var requests = await dbContext.VisualizerRequests
            .Include(request => request.GeneratedPreviews)
            .Where(request =>
                request.UserId == userId &&
                (request.UploadedPhotoUrl != null || request.GeneratedPreviews.Any()))
            .ToListAsync(cancellationToken);

        var referencesToDelete = requests
            .SelectMany(request => request.GeneratedPreviews.Select(preview => preview.ImageUrl))
            .Concat(requests
                .Select(request => request.UploadedPhotoUrl)
                .Where(reference => !string.IsNullOrWhiteSpace(reference))
                .Cast<string>())
            .ToList();

        foreach (var request in requests)
        {
            request.UploadedPhotoUrl = null;
            dbContext.GeneratedPreviews.RemoveRange(request.GeneratedPreviews);
        }

        if (requests.Count > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        foreach (var reference in referencesToDelete)
        {
            fileStorageService.DeletePrivateMedia(reference);
        }
    }

    public async Task<VisualizerRequestDetailsDto> CreateRequestAsync(
        string userId,
        CreateVisualizerRequestDto dto,
        string? uploadedPhotoUrl = null,
        CancellationToken cancellationToken = default)
    {
        var errors = measurementService.Validate(dto.Measurement);
        if (errors.Count > 0)
        {
            throw new InvalidOperationException(string.Join(" ", errors));
        }

        await ValidateCatalogSelectionAsync(dto.DressStyleId, dto.FabricId, dto.FabricColorId, cancellationToken);

        var measurement = new CustomerMeasurement
        {
            UserId = userId,
            Size = dto.Measurement.Size,
            Chest = dto.Measurement.Chest!.Value,
            Waist = dto.Measurement.Waist!.Value,
            ShoulderToShoulder = dto.Measurement.ShoulderToShoulder!.Value,
            ShoulderToWaistSeam = dto.Measurement.ShoulderToWaistSeam!.Value,
            ShoulderToHem = dto.Measurement.ShoulderToHem!.Value,
            SleeveLength = dto.Measurement.SleeveLength!.Value,
            Bicep = dto.Measurement.Bicep!.Value,
            EndOfSleeve = dto.Measurement.EndOfSleeve!.Value,
            ShoulderToSideNeck = dto.Measurement.ShoulderToSideNeck!.Value,
            ShoulderToNeckOpening = dto.Measurement.ShoulderToNeckOpening!.Value,
            MockNeckHeight = dto.Measurement.MockNeckHeight!.Value,
            MockNeckCircumference = dto.Measurement.MockNeckCircumference!.Value,
            UnderarmToSideWaistSeam = dto.Measurement.UnderarmToSideWaistSeam!.Value,
            CenterNeckToWaistPoint = dto.Measurement.CenterNeckToWaistPoint!.Value,
            CuffHeight = dto.Measurement.CuffHeight!.Value,
            SkirtShape = dto.Measurement.SkirtShape,
            HasHorsehairEdgeHem = dto.Measurement.HasHorsehairEdgeHem!.Value,
            NecklineShape = dto.Measurement.NecklineShape,
            SleeveShape = dto.Measurement.SleeveShape,
            HasBuiltInPuffy = dto.Measurement.HasBuiltInPuffy!.Value
        };

        var request = new VisualizerRequest
        {
            UserId = userId,
            DressStyleId = dto.DressStyleId,
            FabricId = dto.FabricId,
            FabricColorId = dto.FabricColorId,
            Measurement = measurement,
            DressTemplate = dto.DesignSelection?.DressTemplate,
            BodiceDesign = dto.DesignSelection?.BodiceDesign,
            WaistShape = dto.DesignSelection?.WaistShape,
            FabricType = dto.DesignSelection?.FabricType,
            FabricPattern = dto.DesignSelection?.FabricPattern,
            Accessories = dto.DesignSelection?.Accessories,
            BackClosure = dto.DesignSelection?.BackClosure,
            UploadedPhotoUrl = uploadedPhotoUrl,
            Status = VisualizerRequestStatus.Draft
        };

        dbContext.VisualizerRequests.Add(request);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetRequestAsync(userId, request.Id, cancellationToken)
            ?? throw new InvalidOperationException("Created request could not be loaded.");
    }

    public async Task<VisualizerRequestDetailsDto?> GetRequestAsync(string userId, Guid requestId, CancellationToken cancellationToken = default)
    {
        var request = await LoadRequestQuery()
            .SingleOrDefaultAsync(candidate => candidate.Id == requestId && candidate.UserId == userId, cancellationToken);

        return request is null ? null : ToDetailsDto(request);
    }

    public async Task<VisualizerRequestDetailsDto?> UpdateStyleAsync(string userId, Guid requestId, Guid styleId, CancellationToken cancellationToken = default)
    {
        var request = await dbContext.VisualizerRequests.SingleOrDefaultAsync(candidate => candidate.Id == requestId && candidate.UserId == userId, cancellationToken);
        if (request is null)
        {
            return null;
        }

        var exists = await dbContext.DressStyles.AnyAsync(style => style.Id == styleId && style.IsActive, cancellationToken);
        if (!exists)
        {
            throw new InvalidOperationException("Selected style is no longer available.");
        }

        request.DressStyleId = styleId;
        request.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetRequestAsync(userId, requestId, cancellationToken);
    }

    public async Task<VisualizerRequestDetailsDto?> UpdateFabricAsync(string userId, Guid requestId, Guid fabricId, Guid colorId, CancellationToken cancellationToken = default)
    {
        var request = await dbContext.VisualizerRequests.SingleOrDefaultAsync(candidate => candidate.Id == requestId && candidate.UserId == userId, cancellationToken);
        if (request is null)
        {
            return null;
        }

        var exists = await dbContext.FabricColors.AnyAsync(color => color.Id == colorId && color.FabricId == fabricId && color.IsActive, cancellationToken);
        if (!exists)
        {
            throw new InvalidOperationException("Selected fabric or color is no longer available.");
        }

        request.FabricId = fabricId;
        request.FabricColorId = colorId;
        request.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetRequestAsync(userId, requestId, cancellationToken);
    }

    public async Task<VisualizerRequestDetailsDto?> UpdateMeasurementsAsync(string userId, Guid requestId, MeasurementDto dto, CancellationToken cancellationToken = default)
    {
        var errors = measurementService.Validate(dto);
        if (errors.Count > 0)
        {
            throw new InvalidOperationException(string.Join(" ", errors));
        }

        var request = await dbContext.VisualizerRequests
            .Include(candidate => candidate.Measurement)
            .SingleOrDefaultAsync(candidate => candidate.Id == requestId && candidate.UserId == userId, cancellationToken);

        if (request?.Measurement is null)
        {
            return null;
        }

        request.Measurement.Size = dto.Size;
        request.Measurement.Chest = dto.Chest!.Value;
        request.Measurement.Waist = dto.Waist!.Value;
        request.Measurement.ShoulderToShoulder = dto.ShoulderToShoulder!.Value;
        request.Measurement.ShoulderToWaistSeam = dto.ShoulderToWaistSeam!.Value;
        request.Measurement.ShoulderToHem = dto.ShoulderToHem!.Value;
        request.Measurement.SleeveLength = dto.SleeveLength!.Value;
        request.Measurement.Bicep = dto.Bicep!.Value;
        request.Measurement.EndOfSleeve = dto.EndOfSleeve!.Value;
        request.Measurement.ShoulderToSideNeck = dto.ShoulderToSideNeck!.Value;
        request.Measurement.ShoulderToNeckOpening = dto.ShoulderToNeckOpening!.Value;
        request.Measurement.MockNeckHeight = dto.MockNeckHeight!.Value;
        request.Measurement.MockNeckCircumference = dto.MockNeckCircumference!.Value;
        request.Measurement.UnderarmToSideWaistSeam = dto.UnderarmToSideWaistSeam!.Value;
        request.Measurement.CenterNeckToWaistPoint = dto.CenterNeckToWaistPoint!.Value;
        request.Measurement.CuffHeight = dto.CuffHeight!.Value;
        request.Measurement.SkirtShape = dto.SkirtShape;
        request.Measurement.HasHorsehairEdgeHem = dto.HasHorsehairEdgeHem!.Value;
        request.Measurement.NecklineShape = dto.NecklineShape;
        request.Measurement.SleeveShape = dto.SleeveShape;
        request.Measurement.HasBuiltInPuffy = dto.HasBuiltInPuffy!.Value;
        request.Measurement.UpdatedAt = DateTimeOffset.UtcNow;
        request.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetRequestAsync(userId, requestId, cancellationToken);
    }

    public async Task<VisualizerRequestDetailsDto?> UpdatePhotoAsync(string userId, Guid requestId, string photoUrl, CancellationToken cancellationToken = default)
    {
        var request = await dbContext.VisualizerRequests.SingleOrDefaultAsync(candidate => candidate.Id == requestId && candidate.UserId == userId, cancellationToken);
        if (request is null)
        {
            return null;
        }

        request.UploadedPhotoUrl = photoUrl;
        request.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetRequestAsync(userId, requestId, cancellationToken);
    }

    public async Task<VisualizerRequestDetailsDto?> SaveBasicPreviewAsync(string userId, Guid requestId, CancellationToken cancellationToken = default)
    {
        var request = await dbContext.VisualizerRequests.SingleOrDefaultAsync(candidate => candidate.Id == requestId && candidate.UserId == userId, cancellationToken);
        if (request is null)
        {
            return null;
        }

        request.BasicPreviewUrl = "/images/visualizer-placeholder.svg";
        request.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetRequestAsync(userId, requestId, cancellationToken);
    }

    public async Task<IReadOnlyList<PreviewHistoryItemDto>> GetHistoryAsync(string userId, CancellationToken cancellationToken = default)
    {
        var history = await dbContext.GeneratedPreviews
            .Include(preview => preview.VisualizerRequest!)
                .ThenInclude(request => request.DressStyle)
            .Include(preview => preview.VisualizerRequest!)
                .ThenInclude(request => request.Fabric)
            .Include(preview => preview.VisualizerRequest!)
                .ThenInclude(request => request.FabricColor)
            .Where(preview => preview.VisualizerRequest!.UserId == userId)
            .OrderByDescending(preview => preview.CreatedAt)
            .Select(preview => new PreviewHistoryItemDto(
                preview.VisualizerRequestId,
                preview.VisualizerRequest!.DressTemplate ?? preview.VisualizerRequest.DressStyle!.Name,
                preview.VisualizerRequest.FabricType ?? preview.VisualizerRequest.Fabric!.Name,
                preview.VisualizerRequest.FabricColor!.ColorName,
                preview.ImageUrl,
                preview.CreatedAt))
            .ToListAsync(cancellationToken);

        return history
            .Select(item => item with { ImageUrl = GetPreviewImageUrl(item.RequestId) })
            .ToList();
    }

    public async Task<string?> GetUploadedPhotoStorageReferenceAsync(
        string userId,
        Guid requestId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.VisualizerRequests
            .Where(request => request.Id == requestId && request.UserId == userId)
            .Select(request => request.UploadedPhotoUrl)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<string?> GetLatestGeneratedPreviewImageUrlAsync(
        string userId,
        Guid requestId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.GeneratedPreviews
            .Where(preview =>
                preview.VisualizerRequestId == requestId &&
                preview.VisualizerRequest!.UserId == userId)
            .OrderByDescending(preview => preview.CreatedAt)
            .Select(preview => preview.ImageUrl)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<GenerateAiPreviewResponse> GenerateAiPreviewAsync(
        string userId,
        GenerateAiPreviewRequest dto,
        CancellationToken cancellationToken = default)
    {
        if (!await usageLimitService.CanGenerateAsync(userId, cancellationToken))
        {
            return new GenerateAiPreviewResponse(dto.VisualizerRequestId, "Failed", null, "Daily AI preview limit reached.");
        }

        var request = await LoadRequestQuery()
            .SingleOrDefaultAsync(candidate => candidate.Id == dto.VisualizerRequestId && candidate.UserId == userId, cancellationToken);

        if (request is null)
        {
            return new GenerateAiPreviewResponse(dto.VisualizerRequestId, "Failed", null, "Visualizer request was not found.");
        }

        if (string.IsNullOrWhiteSpace(request.UploadedPhotoUrl) ||
            fileStorageService.ResolveUploadedPhotoPath(request.UploadedPhotoUrl) is null)
        {
            return new GenerateAiPreviewResponse(
                request.Id,
                "Failed",
                null,
                "Upload a front-facing full-body photo before generating an AI preview.");
        }

        request.Status = VisualizerRequestStatus.Generating;
        request.UpdatedAt = DateTimeOffset.UtcNow;
        request.PromptUsed = promptBuilder.Build(request);
        request.ErrorMessage = null;
        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            var generationRequest = new GownVisualizationRequest(
                ProductId: request.DressStyleId.ToString(),
                ProductTitle: request.DressTemplate ?? request.DressStyle!.Name,
                CustomerImageFileName: request.UploadedPhotoUrl ?? "basic-preview",
                CustomerImageContentType: GetImageContentType(request.UploadedPhotoUrl),
                GarmentImageUrl: null,
                Prompt: request.PromptUsed,
                ImageSize: dto.ImageSize,
                Quality: dto.Quality);

            var result = await imageGenerationService.CreatePreviewAsync(generationRequest, cancellationToken);
            var preview = new GeneratedPreview
            {
                VisualizerRequestId = request.Id,
                ImageUrl = result.PreviewImageUrl.ToString(),
                AiProvider = result.Provider,
                ImageSize = dto.ImageSize ?? "1024x1536",
                Quality = dto.Quality ?? "standard",
                GenerationCost = 0
            };

            request.Status = VisualizerRequestStatus.Completed;
            request.UpdatedAt = DateTimeOffset.UtcNow;

            dbContext.GeneratedPreviews.Add(preview);
            dbContext.AiUsageLogs.Add(new AiUsageLog
            {
                UserId = userId,
                VisualizerRequestId = request.Id,
                Provider = result.Provider,
                Operation = "generate-ai-preview",
                EstimatedInputTokens = request.PromptUsed.Length / 4,
                EstimatedOutputTokens = 0,
                CostEstimate = 0
            });

            await dbContext.SaveChangesAsync(cancellationToken);
            return new GenerateAiPreviewResponse(request.Id, request.Status.ToString(), GetPreviewImageUrl(request.Id), null);
        }
        catch (Exception ex)
        {
            request.Status = VisualizerRequestStatus.Failed;
            request.ErrorMessage = "AI preview generation failed. Please try again.";
            request.UpdatedAt = DateTimeOffset.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            return new GenerateAiPreviewResponse(request.Id, request.Status.ToString(), null, ex.Message);
        }
    }

    private async Task ValidateCatalogSelectionAsync(Guid styleId, Guid fabricId, Guid colorId, CancellationToken cancellationToken)
    {
        var valid = await dbContext.DressStyles.AnyAsync(style => style.Id == styleId && style.IsActive, cancellationToken)
            && await dbContext.Fabrics.AnyAsync(fabric => fabric.Id == fabricId && fabric.IsActive, cancellationToken)
            && await dbContext.FabricColors.AnyAsync(color => color.Id == colorId && color.FabricId == fabricId && color.IsActive, cancellationToken);

        if (!valid)
        {
            throw new InvalidOperationException("Selected style, fabric, or color is no longer available.");
        }
    }

    private static string GetImageContentType(string? photoUrl)
    {
        return Path.GetExtension(photoUrl)?.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "image/png"
        };
    }

    private IQueryable<VisualizerRequest> LoadRequestQuery()
    {
        return dbContext.VisualizerRequests
            .AsSplitQuery()
            .Include(request => request.DressStyle)
            .Include(request => request.Fabric)
                .ThenInclude(fabric => fabric!.Colors)
            .Include(request => request.FabricColor)
            .Include(request => request.Measurement)
            .Include(request => request.GeneratedPreviews);
    }

    private static DressStyleDto ToDto(DressStyle style)
    {
        return new DressStyleDto(style.Id, style.Name, style.Description, style.BasePrice, style.Silhouette, style.Neckline, style.SleeveType, style.DressLength, style.TemplateImageUrl);
    }

    private static FabricDto ToDto(Fabric fabric)
    {
        return new FabricDto(
            fabric.Id,
            fabric.Name,
            fabric.Description,
            fabric.MaterialType,
            fabric.Colors.Where(color => color.IsActive).Select(ToDto).ToList());
    }

    private static FabricColorDto ToDto(FabricColor color)
    {
        return new FabricColorDto(color.Id, color.ColorName, color.HexCode, color.SwatchImageUrl);
    }

    private static VisualizerRequestDetailsDto ToDetailsDto(VisualizerRequest request)
    {
        var measurement = request.Measurement!;
        var latestPreview = request.GeneratedPreviews.OrderByDescending(preview => preview.CreatedAt).FirstOrDefault();

        return new VisualizerRequestDetailsDto(
            request.Id,
            request.UserId,
            ToDto(request.DressStyle!),
            ToDto(request.Fabric!),
            ToDto(request.FabricColor!),
            new MeasurementDto
            {
                Size = measurement.Size,
                Chest = measurement.Chest,
                Waist = measurement.Waist,
                ShoulderToShoulder = measurement.ShoulderToShoulder,
                ShoulderToWaistSeam = measurement.ShoulderToWaistSeam,
                ShoulderToHem = measurement.ShoulderToHem,
                SleeveLength = measurement.SleeveLength,
                Bicep = measurement.Bicep,
                EndOfSleeve = measurement.EndOfSleeve,
                ShoulderToSideNeck = measurement.ShoulderToSideNeck,
                ShoulderToNeckOpening = measurement.ShoulderToNeckOpening,
                MockNeckHeight = measurement.MockNeckHeight,
                MockNeckCircumference = measurement.MockNeckCircumference,
                UnderarmToSideWaistSeam = measurement.UnderarmToSideWaistSeam,
                CenterNeckToWaistPoint = measurement.CenterNeckToWaistPoint,
                CuffHeight = measurement.CuffHeight,
                SkirtShape = measurement.SkirtShape,
                HasHorsehairEdgeHem = measurement.HasHorsehairEdgeHem,
                NecklineShape = measurement.NecklineShape,
                SleeveShape = measurement.SleeveShape,
                HasBuiltInPuffy = measurement.HasBuiltInPuffy
            },
            HasDesignSelection(request)
                ? new DesignSelectionDto
                {
                    DressTemplate = request.DressTemplate ?? string.Empty,
                    BodiceDesign = request.BodiceDesign ?? string.Empty,
                    WaistShape = request.WaistShape ?? string.Empty,
                    FabricType = request.FabricType ?? string.Empty,
                    FabricPattern = request.FabricPattern ?? string.Empty,
                    Accessories = request.Accessories ?? string.Empty,
                    BackClosure = request.BackClosure ?? string.Empty
                }
                : null,
            request.UploadedPhotoUrl is null ? null : GetUploadedPhotoImageUrl(request.Id),
            request.BasicPreviewUrl,
            request.PromptUsed,
            request.Status,
            request.ErrorMessage,
            latestPreview is null ? null : GetPreviewImageUrl(request.Id),
            request.CreatedAt);
    }

    private static bool HasDesignSelection(VisualizerRequest request)
    {
        return !string.IsNullOrWhiteSpace(request.DressTemplate)
            || !string.IsNullOrWhiteSpace(request.BodiceDesign)
            || !string.IsNullOrWhiteSpace(request.WaistShape)
            || !string.IsNullOrWhiteSpace(request.FabricType)
            || !string.IsNullOrWhiteSpace(request.FabricPattern)
            || !string.IsNullOrWhiteSpace(request.Accessories)
            || !string.IsNullOrWhiteSpace(request.BackClosure);
    }

    private static string GetUploadedPhotoImageUrl(Guid requestId)
    {
        return $"/api/visualizer/request/{requestId}/uploaded-photo";
    }

    private static string GetPreviewImageUrl(Guid requestId)
    {
        return $"/api/visualizer/request/{requestId}/preview-image";
    }
}
