namespace indexApp.Features.Shopify;

public sealed class ShopifyOptions
{
    public const string SectionName = "Shopify";

    public string? StoreDomain { get; init; }

    public string? AdminAccessToken { get; init; }

    public string ApiVersion { get; init; } = "2026-04";

    public string VisualizerMetafieldNamespace { get; init; } = "ai_visualizer";
}
