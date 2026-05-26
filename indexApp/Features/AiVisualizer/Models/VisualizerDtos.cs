using System.ComponentModel.DataAnnotations;
using indexApp.Features.AiVisualizer.Entities;

namespace indexApp.Features.AiVisualizer.Models;

public sealed record DressStyleDto(
    Guid Id,
    string Name,
    string Description,
    decimal BasePrice,
    string Silhouette,
    string Neckline,
    string SleeveType,
    string DressLength,
    string? TemplateImageUrl);

public sealed record FabricDto(
    Guid Id,
    string Name,
    string Description,
    string MaterialType,
    IReadOnlyList<FabricColorDto> Colors);

public sealed record FabricColorDto(
    Guid Id,
    string ColorName,
    string HexCode,
    string? SwatchImageUrl);

public sealed class MeasurementDto
{
    [Required]
    public string Size { get; set; } = string.Empty;

    [Required, Range(50, 205)]
    public decimal? Chest { get; set; }

    [Required, Range(45, 180)]
    public decimal? Waist { get; set; }

    [Required, Range(20, 82)]
    public decimal? ShoulderToShoulder { get; set; }

    [Required, Range(20, 80)]
    public decimal? ShoulderToWaistSeam { get; set; }

    [Required, Range(50, 230)]
    public decimal? ShoulderToHem { get; set; }

    [Required, Range(0, 102)]
    public decimal? SleeveLength { get; set; }

    [Required, Range(15, 70)]
    public decimal? Bicep { get; set; }

    [Required, Range(10, 41)]
    public decimal? EndOfSleeve { get; set; }

    [Required, Range(5, 35)]
    public decimal? ShoulderToSideNeck { get; set; }

    [Required, Range(5, 60)]
    public decimal? ShoulderToNeckOpening { get; set; }

    [Required, Range(0, 25)]
    public decimal? MockNeckHeight { get; set; }

    [Required, Range(0, 80)]
    public decimal? MockNeckCircumference { get; set; }

    [Required, Range(5, 60)]
    public decimal? UnderarmToSideWaistSeam { get; set; }

    [Required, Range(15, 90)]
    public decimal? CenterNeckToWaistPoint { get; set; }

    [Required, Range(0, 30)]
    public decimal? CuffHeight { get; set; }

    [Required]
    public string SkirtShape { get; set; } = string.Empty;

    public bool HasHorsehairEdgeHem { get; set; }

    [Required]
    public string NecklineShape { get; set; } = string.Empty;

    [Required]
    public string SleeveShape { get; set; } = string.Empty;

    public bool HasBuiltInPuffy { get; set; }
}

public sealed class CreateVisualizerRequestDto
{
    public Guid DressStyleId { get; set; }
    public Guid FabricId { get; set; }
    public Guid FabricColorId { get; set; }
    public MeasurementDto Measurement { get; set; } = new();
}

public sealed record GenerateAiPreviewRequest(
    Guid VisualizerRequestId,
    string? Quality,
    string? ImageSize);

public sealed record GenerateAiPreviewResponse(
    Guid VisualizerRequestId,
    string Status,
    string? ImageUrl,
    string? ErrorMessage);

public sealed record VisualizerRequestDetailsDto(
    Guid Id,
    string UserId,
    DressStyleDto DressStyle,
    FabricDto Fabric,
    FabricColorDto FabricColor,
    MeasurementDto Measurement,
    string? UploadedPhotoUrl,
    string? BasicPreviewUrl,
    string? PromptUsed,
    VisualizerRequestStatus Status,
    string? ErrorMessage,
    string? LatestPreviewUrl,
    DateTimeOffset CreatedAt);

public sealed record PreviewHistoryItemDto(
    Guid RequestId,
    string StyleName,
    string FabricName,
    string ColorName,
    string ImageUrl,
    DateTimeOffset CreatedAt);
