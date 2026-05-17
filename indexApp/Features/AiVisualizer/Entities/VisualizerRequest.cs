namespace indexApp.Features.AiVisualizer.Entities;

public sealed class VisualizerRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public Guid DressStyleId { get; set; }
    public Guid FabricId { get; set; }
    public Guid FabricColorId { get; set; }
    public Guid MeasurementId { get; set; }
    public string? UploadedPhotoUrl { get; set; }
    public string? BasicPreviewUrl { get; set; }
    public string? PromptUsed { get; set; }
    public VisualizerRequestStatus Status { get; set; } = VisualizerRequestStatus.Draft;
    public string? ErrorMessage { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DressStyle? DressStyle { get; set; }
    public Fabric? Fabric { get; set; }
    public FabricColor? FabricColor { get; set; }
    public CustomerMeasurement? Measurement { get; set; }
    public ICollection<GeneratedPreview> GeneratedPreviews { get; set; } = [];
}
