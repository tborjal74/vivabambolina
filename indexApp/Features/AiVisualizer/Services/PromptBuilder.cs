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
            $"Edit the provided front-facing full-body photo so the same person is realistically wearing a {color.ColorName} {fabric.Name} {style.Name}. " +
            "Preserve the person's face, identity, skin tone, hair, body position, proportions, and visible background; change only the outfit as needed for the gown visualization. " +
            "Compose the output as a full-length portrait with the entire head, hair, body, gown hem, and both feet visible inside the frame. Do not crop the head, face, feet, or dress hem, and do not zoom in. " +
            $"Use the selected gown design: silhouette {style.Silhouette}, neckline {style.Neckline}, sleeve type {style.SleeveType}, dress length {style.DressLength}. " +
            $"Reflect approximate body proportions: height {measurement.Height}, bust {measurement.Bust}, waist {measurement.Waist}, hips {measurement.Hips}, " +
            $"shoulder width {measurement.ShoulderWidth}, sleeve length {measurement.SleeveLength}, wrist {measurement.Wrist}. " +
            "Keep the result tasteful, realistic, front-facing, and focused on how the gown appears on the photographed person.";
    }
}
