namespace indexApp.Features.AiVisualizer.Entities;

public sealed class CustomerMeasurement
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public decimal Height { get; set; }
    public decimal Bust { get; set; }
    public decimal Waist { get; set; }
    public decimal Hips { get; set; }
    public decimal ShoulderWidth { get; set; }
    public decimal DressLength { get; set; }
    public decimal SleeveLength { get; set; }
    public decimal Wrist { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
