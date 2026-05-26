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
            $"Use garment size {measurement.Size} and construction choices: {measurement.SkirtShape} skirt, {measurement.NecklineShape} neckline, {measurement.SleeveShape} sleeve, " +
            $"horsehair edge hem {(measurement.HasHorsehairEdgeHem ? "yes" : "no")}, built-in puffy {(measurement.HasBuiltInPuffy ? "yes" : "no")}. " +
            $"Reflect approximate garment measurements in centimeters: chest {measurement.Chest} cm, waist {measurement.Waist} cm, shoulder to shoulder {measurement.ShoulderToShoulder} cm, " +
            $"shoulder to waist seam {measurement.ShoulderToWaistSeam} cm, shoulder to hem {measurement.ShoulderToHem} cm, sleeve length {measurement.SleeveLength} cm, " +
            $"bicep {measurement.Bicep} cm, end of sleeve {measurement.EndOfSleeve} cm, shoulder to side neck {measurement.ShoulderToSideNeck} cm, " +
            $"shoulder to neck opening {measurement.ShoulderToNeckOpening} cm, mock neck height {measurement.MockNeckHeight} cm, mock neck circumference {measurement.MockNeckCircumference} cm, " +
            $"underarm to side waist seam {measurement.UnderarmToSideWaistSeam} cm, center neck to waist point {measurement.CenterNeckToWaistPoint} cm, cuff height {measurement.CuffHeight} cm. " +
            "Keep the result tasteful, realistic, front-facing, and focused on how the gown appears on the photographed person.";
    }
}
