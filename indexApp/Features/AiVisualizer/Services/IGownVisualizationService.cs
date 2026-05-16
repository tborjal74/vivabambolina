using indexApp.Features.AiVisualizer.Models;

namespace indexApp.Features.AiVisualizer.Services;

public interface IGownVisualizationService
{
    Task<GownVisualizationResult> CreatePreviewAsync(
        GownVisualizationRequest request,
        CancellationToken cancellationToken = default);
}
