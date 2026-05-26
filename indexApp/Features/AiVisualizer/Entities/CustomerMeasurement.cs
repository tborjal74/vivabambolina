namespace indexApp.Features.AiVisualizer.Entities;

public sealed class CustomerMeasurement
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public decimal Chest { get; set; }
    public decimal Waist { get; set; }
    public decimal ShoulderToShoulder { get; set; }
    public decimal ShoulderToWaistSeam { get; set; }
    public decimal ShoulderToHem { get; set; }
    public decimal SleeveLength { get; set; }
    public decimal Bicep { get; set; }
    public decimal EndOfSleeve { get; set; }
    public decimal ShoulderToSideNeck { get; set; }
    public decimal ShoulderToNeckOpening { get; set; }
    public decimal MockNeckHeight { get; set; }
    public decimal MockNeckCircumference { get; set; }
    public decimal UnderarmToSideWaistSeam { get; set; }
    public decimal CenterNeckToWaistPoint { get; set; }
    public decimal CuffHeight { get; set; }
    public string SkirtShape { get; set; } = string.Empty;
    public bool HasHorsehairEdgeHem { get; set; }
    public string NecklineShape { get; set; } = string.Empty;
    public string SleeveShape { get; set; } = string.Empty;
    public bool HasBuiltInPuffy { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
