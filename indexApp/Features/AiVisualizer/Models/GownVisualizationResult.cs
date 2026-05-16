namespace indexApp.Features.AiVisualizer.Models;

public sealed record GownVisualizationResult(
    string JobId,
    string Status,
    Uri PreviewImageUrl,
    string Provider,
    DateTimeOffset CreatedAt,
    IReadOnlyList<string> Warnings);
