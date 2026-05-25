using indexApp.Features.AiVisualizer.Models;

namespace indexApp.Features.AiVisualizer.Services;

public sealed class MeasurementService
{
    public IReadOnlyList<string> Validate(MeasurementDto measurement)
    {
        var errors = new List<string>();
        Check(measurement.Height, 90, 245, "Height", errors);
        Check(measurement.Bust, 50, 205, "Bust", errors);
        Check(measurement.Waist, 45, 180, "Waist", errors);
        Check(measurement.Hips, 50, 220, "Hips", errors);
        Check(measurement.ShoulderWidth, 20, 82, "Shoulder width", errors);
        Check(measurement.DressLength, 50, 230, "Dress length", errors);
        Check(measurement.SleeveLength, 0, 102, "Sleeve length", errors);
        Check(measurement.Wrist, 10, 41, "Wrist", errors);
        return errors;
    }

    public MannequinScale GetScale(MeasurementDto measurement)
    {
        return new MannequinScale(
            Height: Clamp((measurement.Height - 152.4m) / 30.48m, -0.18m, 0.22m),
            Bust: Clamp((measurement.Bust - 86.36m) / 101.6m, -0.12m, 0.18m),
            Waist: Clamp((measurement.Waist - 68.58m) / 88.9m, -0.16m, 0.16m),
            Hips: Clamp((measurement.Hips - 91.44m) / 101.6m, -0.12m, 0.18m),
            Shoulder: Clamp((measurement.ShoulderWidth - 38.1m) / 50.8m, -0.12m, 0.16m),
            DressLength: Clamp((measurement.DressLength - 147.32m) / 177.8m, -0.12m, 0.16m),
            SleeveLength: Clamp(measurement.SleeveLength / 101.6m, 0m, 1m),
            Wrist: Clamp((measurement.Wrist - 15.24m) / 40.64m, -0.08m, 0.12m));
    }

    private static void Check(decimal value, decimal min, decimal max, string label, ICollection<string> errors)
    {
        if (value < min || value > max)
        {
            errors.Add($"{label} must be between {min} and {max} centimeters.");
        }
    }

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
