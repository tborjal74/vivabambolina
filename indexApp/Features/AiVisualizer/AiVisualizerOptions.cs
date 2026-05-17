namespace indexApp.Features.AiVisualizer;

public sealed class AiVisualizerOptions
{
    public const string SectionName = "AiVisualizer";

    public string Provider { get; init; } = "Stub";

    public string? ApiKey { get; init; }

    public string? Endpoint { get; init; } = "https://api.openai.com/v1/images/generations";

    public string Model { get; init; } = "gpt-image-1";

    public string ImageSize { get; init; } = "1024x1024";

    public string ImageQuality { get; init; } = "medium";

    public string StoragePath { get; init; } = "wwwroot/uploads/visualizer";

    public long MaxUploadBytes { get; init; } = 10 * 1024 * 1024;

    public int MaxAiPreviewsPerUserPerDay { get; init; } = 5;

    public string[] AllowedImageContentTypes { get; init; } =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];
}
