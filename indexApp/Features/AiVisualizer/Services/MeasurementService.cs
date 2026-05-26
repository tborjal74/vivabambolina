using indexApp.Features.AiVisualizer.Models;

namespace indexApp.Features.AiVisualizer.Services;

public sealed class MeasurementService
{
    public static readonly IReadOnlyList<string> Sizes = ["S", "M", "L", "XL", "XXL"];
    public static readonly IReadOnlyList<string> SkirtShapes = ["A-Line", "Ball Gown", "Mermaid", "Column", "Circle"];
    public static readonly IReadOnlyList<string> NecklineShapes = ["Sweetheart", "V-Neck", "Scoop", "Boat", "Off-Shoulder", "Halter", "Mock Neck"];
    public static readonly IReadOnlyList<string> SleeveShapes = ["Sleeveless", "Cap Sleeve", "Short Sleeve", "Puff Sleeve", "Bishop Sleeve", "Long Fitted Sleeve"];

    public IReadOnlyList<string> Validate(MeasurementDto measurement)
    {
        var errors = new List<string>();
        CheckChoice(measurement.Size, Sizes, "Size", errors);
        Check(measurement.Chest, 50, 205, "Chest", errors);
        Check(measurement.Waist, 45, 180, "Waist", errors);
        Check(measurement.ShoulderToShoulder, 20, 82, "Shoulder to shoulder", errors);
        Check(measurement.ShoulderToWaistSeam, 20, 80, "Shoulder to waist seam", errors);
        Check(measurement.ShoulderToHem, 50, 230, "Shoulder to hem", errors);
        Check(measurement.SleeveLength, 0, 102, "Sleeve length", errors);
        Check(measurement.Bicep, 15, 70, "Bicep", errors);
        Check(measurement.EndOfSleeve, 10, 41, "End of sleeve", errors);
        Check(measurement.ShoulderToSideNeck, 5, 35, "Shoulder to side neck", errors);
        Check(measurement.ShoulderToNeckOpening, 5, 60, "Shoulder to neck opening", errors);
        Check(measurement.MockNeckHeight, 0, 25, "Mock neck height", errors);
        Check(measurement.MockNeckCircumference, 0, 80, "Mock neck circumference", errors);
        Check(measurement.UnderarmToSideWaistSeam, 5, 60, "Underarm to side waist seam", errors);
        Check(measurement.CenterNeckToWaistPoint, 15, 90, "Center neck to waist point", errors);
        Check(measurement.CuffHeight, 0, 30, "Cuff height", errors);
        CheckChoice(measurement.SkirtShape, SkirtShapes, "Skirt shape", errors);
        CheckChoice(measurement.NecklineShape, NecklineShapes, "Neckline shape", errors);
        CheckChoice(measurement.SleeveShape, SleeveShapes, "Sleeve shape", errors);
        return errors;
    }

    public MannequinScale GetScale(MeasurementDto measurement)
    {
        var chest = measurement.Chest ?? 86.36m;
        var waist = measurement.Waist ?? 68.58m;
        var shoulderToShoulder = measurement.ShoulderToShoulder ?? 38.1m;
        var shoulderToHem = measurement.ShoulderToHem ?? 147.32m;
        var sleeveLength = measurement.SleeveLength ?? 56m;
        var endOfSleeve = measurement.EndOfSleeve ?? 15.24m;

        return new MannequinScale(
            Height: Clamp((shoulderToHem - 147.32m) / 177.8m, -0.18m, 0.22m),
            Bust: Clamp((chest - 86.36m) / 101.6m, -0.12m, 0.18m),
            Waist: Clamp((waist - 68.58m) / 88.9m, -0.16m, 0.16m),
            Hips: GetSkirtVolume(measurement.SkirtShape),
            Shoulder: Clamp((shoulderToShoulder - 38.1m) / 50.8m, -0.12m, 0.16m),
            DressLength: Clamp((shoulderToHem - 147.32m) / 177.8m, -0.12m, 0.16m),
            SleeveLength: Clamp(sleeveLength / 101.6m, 0m, 1m),
            Wrist: Clamp((endOfSleeve - 15.24m) / 40.64m, -0.08m, 0.12m));
    }

    private static void Check(decimal? value, decimal min, decimal max, string label, ICollection<string> errors)
    {
        if (value is null)
        {
            errors.Add($"{label} is required.");
            return;
        }

        if (value < min || value > max)
        {
            errors.Add($"{label} must be between {min} and {max} centimeters.");
        }
    }

    private static void CheckChoice(string value, IReadOnlyList<string> allowedValues, string label, ICollection<string> errors)
    {
        if (!allowedValues.Contains(value, StringComparer.OrdinalIgnoreCase))
        {
            errors.Add($"Select a valid {label.ToLowerInvariant()}.");
        }
    }

    private static decimal GetSkirtVolume(string skirtShape) => skirtShape switch
    {
        "Ball Gown" => 0.18m,
        "Circle" => 0.14m,
        "A-Line" => 0.08m,
        "Mermaid" => -0.05m,
        "Column" => -0.09m,
        _ => 0m
    };

    private static decimal Clamp(decimal value, decimal min, decimal max) => Math.Min(max, Math.Max(min, value));
}

public sealed record MannequinScale(
    decimal Height,
    decimal Bust,
    decimal Waist,
    decimal Hips,
    decimal Shoulder,
    decimal DressLength,
    decimal SleeveLength,
    decimal Wrist);
