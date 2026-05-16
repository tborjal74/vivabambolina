namespace indexApp.Features.AiVisualizer;

public sealed class AiVisualizerOptions
{
    public const string SectionName = "AiVisualizer";

    public string Provider { get; init; } = "Stub";

    public string? ApiKey { get; init; }

    public string? Endpoint { get; init; }

    public string Model { get; init; } = "gpt-image-1";

    public string StoragePath { get; init; } = "wwwroot/uploads/visualizer";

    public long MaxUploadBytes { get; init; } = 10 * 1024 * 1024;

    public string[] AllowedImageContentTypes { get; init; } =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];
}
