using indexApp.Features.AiVisualizer.Entities;

namespace indexApp.Features.AiVisualizer.Services;

public sealed class PromptBuilder
{
    public string Build(VisualizerRequest request)
    {
        var style = request.DressStyle ?? throw new InvalidOperationException("Request style was not loaded.");
        var fabric = request.Fabric ?? throw new InvalidOperationException("Request fabric was not loaded.");
        var color = request.FabricColor ?? throw new InvalidOperationException("Request color was not loaded.");
        var measurement = request.Measurement ?? throw new InvalidOperationException("Request measurement was not loaded.");

        return
            $"Create a clean fashion product visualization of a mannequin wearing a {color.ColorName} {fabric.Name} {style.Name}. " +
            $"Use the selected gown style exactly: silhouette {style.Silhouette}, neckline {style.Neckline}, sleeve type {style.SleeveType}, dress length {style.DressLength}. " +
            $"Reflect approximate body proportions: height {measurement.Height}, bust {measurement.Bust}, waist {measurement.Waist}, hips {measurement.Hips}, " +
            $"shoulder width {measurement.ShoulderWidth}, sleeve length {measurement.SleeveLength}, wrist {measurement.Wrist}. " +
            "Keep the image tasteful, boutique-lit, front-facing, realistic, and focused on gown visualization only. Do not alter identity from reference photos.";
    }
}
