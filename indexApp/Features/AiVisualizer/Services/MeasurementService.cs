using indexApp.Features.AiVisualizer.Models;

namespace indexApp.Features.AiVisualizer.Services;

public sealed class MeasurementService
{
    public IReadOnlyList<string> Validate(MeasurementDto measurement)
    {
        var errors = new List<string>();
        Check(measurement.Height, 36, 96, "Height", errors);
        Check(measurement.Bust, 20, 80, "Bust", errors);
        Check(measurement.Waist, 18, 70, "Waist", errors);
        Check(measurement.Hips, 20, 85, "Hips", errors);
        Check(measurement.ShoulderWidth, 8, 32, "Shoulder width", errors);
        Check(measurement.DressLength, 20, 90, "Dress length", errors);
        Check(measurement.SleeveLength, 0, 40, "Sleeve length", errors);
        Check(measurement.Wrist, 4, 16, "Wrist", errors);
        return errors;
    }

    public MannequinScale GetScale(MeasurementDto measurement)
    {
        return new MannequinScale(
            Height: Clamp((measurement.Height - 60) / 12, -0.18m, 0.22m),
            Bust: Clamp((measurement.Bust - 34) / 40, -0.12m, 0.18m),
            Waist: Clamp((measurement.Waist - 27) / 35, -0.16m, 0.16m),
            Hips: Clamp((measurement.Hips - 36) / 40, -0.12m, 0.18m),
            Shoulder: Clamp((measurement.ShoulderWidth - 15) / 20, -0.12m, 0.16m),
            DressLength: Clamp((measurement.DressLength - 58) / 70, -0.12m, 0.16m),
            SleeveLength: Clamp(measurement.SleeveLength / 40, 0m, 1m),
            Wrist: Clamp((measurement.Wrist - 6) / 16, -0.08m, 0.12m));
    }

    private static void Check(decimal value, decimal min, decimal max, string label, ICollection<string> errors)
    {
        if (value < min || value > max)
        {
            errors.Add($"{label} must be between {min} and {max} inches.");
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
