namespace indexApp.Features.AiVisualizer.Models;

public sealed record GownVisualizationRequest(
    string ProductId,
    string ProductTitle,
    string CustomerImageFileName,
    string CustomerImageContentType,
    Uri? GarmentImageUrl,
    string? Prompt,
    string? ImageSize = null,
    string? Quality = null);
