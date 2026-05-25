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
    [Range(90, 245)]
    public decimal Height { get; set; } = 165;

    [Range(50, 205)]
    public decimal Bust { get; set; } = 86;

    [Range(45, 180)]
    public decimal Waist { get; set; } = 66;

    [Range(50, 220)]
    public decimal Hips { get; set; } = 91;

    [Range(20, 82)]
    public decimal ShoulderWidth { get; set; } = 38;

    [Range(50, 230)]
    public decimal DressLength { get; set; } = 147;

    [Range(0, 102)]
    public decimal SleeveLength { get; set; } = 56;

    [Range(10, 41)]
    public decimal Wrist { get; set; } = 15;
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
