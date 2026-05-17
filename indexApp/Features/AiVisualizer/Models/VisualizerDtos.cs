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
    [Range(36, 96)]
    public decimal Height { get; set; } = 65;

    [Range(20, 80)]
    public decimal Bust { get; set; } = 34;

    [Range(18, 70)]
    public decimal Waist { get; set; } = 26;

    [Range(20, 85)]
    public decimal Hips { get; set; } = 36;

    [Range(8, 32)]
    public decimal ShoulderWidth { get; set; } = 15;

    [Range(20, 90)]
    public decimal DressLength { get; set; } = 58;

    [Range(0, 40)]
    public decimal SleeveLength { get; set; } = 22;

    [Range(4, 16)]
    public decimal Wrist { get; set; } = 6;
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
