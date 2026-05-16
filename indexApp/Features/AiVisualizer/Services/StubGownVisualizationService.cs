using indexApp.Features.AiVisualizer.Models;
using Microsoft.Extensions.Options;

namespace indexApp.Features.AiVisualizer.Services;

public sealed class StubGownVisualizationService(
    IOptions<AiVisualizerOptions> options,
    ILogger<StubGownVisualizationService> logger) : IGownVisualizationService
{
    private readonly AiVisualizerOptions _options = options.Value;

    public Task<GownVisualizationResult> CreatePreviewAsync(
        GownVisualizationRequest request,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Created stub gown visualization for product {ProductId} with provider {Provider}.",
            request.ProductId,
            _options.Provider);

        var result = new GownVisualizationResult(
            JobId: $"stub_{Guid.NewGuid():N}",
            Status: "Ready",
            PreviewImageUrl: new Uri("/images/visualizer-placeholder.svg", UriKind.Relative),
            Provider: _options.Provider,
            CreatedAt: DateTimeOffset.UtcNow,
            Warnings:
            [
                "This is a local stub result. Configure a real image provider before production use."
            ]);

        return Task.FromResult(result);
    }
}
