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
            "Blocked photo-based gown visualization for product {ProductId} because provider {Provider} is configured.",
            request.ProductId,
            _options.Provider);

        throw new InvalidOperationException(
            "AI try-on generation is unavailable while AiVisualizer:Provider is Stub. Configure the OpenAI provider to generate a preview from the uploaded photo.");
    }
}
