namespace indexApp.Features.AiVisualizer;

public sealed class AiVisualizerOptions
{
    public const string SectionName = "AiVisualizer";

    public string Provider { get; init; } = "Stub";

    public string? ApiKey { get; init; }

    public string? Endpoint { get; init; } = "https://api.openai.com/v1/images/generations";

    public string? EditEndpoint { get; init; } = "https://api.openai.com/v1/images/edits";

    public string Model { get; init; } = "gpt-image-1.5";

    public string ImageSize { get; init; } = "1024x1536";

    public string ImageQuality { get; init; } = "medium";

    public string StoragePath { get; init; } = "App_Data/visualizer";

    public int PrivateMediaRetentionDays { get; init; } = 30;

    public long MaxUploadBytes { get; init; } = 10 * 1024 * 1024;

    public int MaxAiPreviewsPerUserPerDay { get; init; } = 10;

    public string[] AllowedImageContentTypes { get; init; } =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];
}
