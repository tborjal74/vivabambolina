using indexApp.Features.AiVisualizer.Entities;

namespace indexApp.Features.AiVisualizer.Services;

public sealed class PromptBuilder
{
    public string Build(VisualizerRequest request, PreviewView view = PreviewView.Front)
    {
        var style = request.DressStyle ?? throw new InvalidOperationException("Request style was not loaded.");
        var fabric = request.Fabric ?? throw new InvalidOperationException("Request fabric was not loaded.");
        var color = request.FabricColor ?? throw new InvalidOperationException("Request color was not loaded.");
        var measurement = request.Measurement ?? throw new InvalidOperationException("Request measurement was not loaded.");
        var dressTemplate = request.DressTemplate ?? style.Name;
        var fabricType = request.FabricType ?? fabric.Name;

        var poseDirection = view == PreviewView.Back
            ? "Generate a back-view full-length portrait of the same person wearing this gown, showing the rear gown construction and back closure clearly. Preserve hair, body proportions, skin tone, and background appearance consistently with the uploaded person. "
            : "Generate a front-view full-length portrait of the same person wearing this gown, showing the neckline and front bodice clearly. Preserve the person's face, identity, skin tone, hair, body position, proportions, and visible background. ";

        return
            $"Edit the provided front-facing full-body photo so the same person is realistically wearing a {color.ColorName} {fabricType} {dressTemplate}. " +
            poseDirection +
            "Change only the outfit and viewing direction as needed for the gown visualization. " +
            "Compose the output as a full-length portrait with the entire head, hair, body, gown hem, and both feet visible inside the frame. Do not crop the head, face, feet, or dress hem, and do not zoom in. " +
            $"Use the selected gown design: neckline {measurement.NecklineShape}, sleeve type {measurement.SleeveShape}, skirt design {measurement.SkirtShape}, bodice {request.BodiceDesign ?? style.Silhouette}, waist {request.WaistShape ?? "natural waist"}, dress length {style.DressLength}. " +
            $"Use fabric texture or pattern {request.FabricPattern ?? fabric.Description}, accessories {request.Accessories ?? "none"}, and back closure {request.BackClosure ?? "standard concealed closure"}. " +
            $"Use garment size {measurement.Size} and additional construction choices: " +
            $"horsehair edge hem {(measurement.HasHorsehairEdgeHem ? "yes" : "no")}, built-in puffy {(measurement.HasBuiltInPuffy ? "yes" : "no")}. " +
            $"Reflect approximate garment measurements in centimeters: chest {measurement.Chest} cm, waist {measurement.Waist} cm, shoulder to shoulder {measurement.ShoulderToShoulder} cm, " +
            $"shoulder to waist seam {measurement.ShoulderToWaistSeam} cm, shoulder to hem {measurement.ShoulderToHem} cm, sleeve length {measurement.SleeveLength} cm, " +
            $"bicep {measurement.Bicep} cm, end of sleeve {measurement.EndOfSleeve} cm, shoulder to side neck {measurement.ShoulderToSideNeck} cm, " +
            $"shoulder to neck opening {measurement.ShoulderToNeckOpening} cm, mock neck height {measurement.MockNeckHeight} cm, mock neck circumference {measurement.MockNeckCircumference} cm, " +
            $"underarm to side waist seam {measurement.UnderarmToSideWaistSeam} cm, center neck to waist point {measurement.CenterNeckToWaistPoint} cm, cuff height {measurement.CuffHeight} cm. " +
            $"Keep the result tasteful, realistic, shown from the {view.ToString().ToLowerInvariant()}, and focused on how the gown appears on the photographed person.";
    }
}
