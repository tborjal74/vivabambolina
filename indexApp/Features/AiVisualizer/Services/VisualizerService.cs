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
    private readonly AiVisualizerOptions options;

    public VisualizerService(
        AiVisualizerDbContext dbContext,
        IGownVisualizationService imageGenerationService,
        MeasurementService measurementService,
        PromptBuilder promptBuilder,
        UsageLimitService usageLimitService,
        IOptions<AiVisualizerOptions> options)
    {
        this.dbContext = dbContext;
        this.imageGenerationService = imageGenerationService;
        this.measurementService = measurementService;
        this.promptBuilder = promptBuilder;
        this.usageLimitService = usageLimitService;
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
            Height = dto.Measurement.Height,
            Bust = dto.Measurement.Bust,
            Waist = dto.Measurement.Waist,
            Hips = dto.Measurement.Hips,
            ShoulderWidth = dto.Measurement.ShoulderWidth,
            DressLength = dto.Measurement.DressLength,
            SleeveLength = dto.Measurement.SleeveLength,
            Wrist = dto.Measurement.Wrist
        };

        var request = new VisualizerRequest
        {
            UserId = userId,
            DressStyleId = dto.DressStyleId,
            FabricId = dto.FabricId,
            FabricColorId = dto.FabricColorId,
            Measurement = measurement,
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

        request.Measurement.Height = dto.Height;
        request.Measurement.Bust = dto.Bust;
        request.Measurement.Waist = dto.Waist;
        request.Measurement.Hips = dto.Hips;
        request.Measurement.ShoulderWidth = dto.ShoulderWidth;
        request.Measurement.DressLength = dto.DressLength;
        request.Measurement.SleeveLength = dto.SleeveLength;
        request.Measurement.Wrist = dto.Wrist;
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
        return await dbContext.GeneratedPreviews
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
                preview.VisualizerRequest!.DressStyle!.Name,
                preview.VisualizerRequest.Fabric!.Name,
                preview.VisualizerRequest.FabricColor!.ColorName,
                preview.ImageUrl,
                preview.CreatedAt))
            .ToListAsync(cancellationToken);
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

        request.Status = VisualizerRequestStatus.Generating;
        request.UpdatedAt = DateTimeOffset.UtcNow;
        request.PromptUsed = promptBuilder.Build(request);
        request.ErrorMessage = null;
        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            var generationRequest = new GownVisualizationRequest(
                ProductId: request.DressStyleId.ToString(),
                ProductTitle: request.DressStyle!.Name,
                CustomerImageFileName: request.UploadedPhotoUrl ?? "basic-preview",
                CustomerImageContentType: "image/svg+xml",
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
                ImageSize = dto.ImageSize ?? "1024x1024",
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
            return new GenerateAiPreviewResponse(request.Id, request.Status.ToString(), preview.ImageUrl, null);
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

    private IQueryable<VisualizerRequest> LoadRequestQuery()
    {
        return dbContext.VisualizerRequests
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
                Height = measurement.Height,
                Bust = measurement.Bust,
                Waist = measurement.Waist,
                Hips = measurement.Hips,
                ShoulderWidth = measurement.ShoulderWidth,
                DressLength = measurement.DressLength,
                SleeveLength = measurement.SleeveLength,
                Wrist = measurement.Wrist
            },
            request.UploadedPhotoUrl,
            request.BasicPreviewUrl,
            request.PromptUsed,
            request.Status,
            request.ErrorMessage,
            latestPreview?.ImageUrl,
            request.CreatedAt);
    }
}
