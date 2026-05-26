namespace indexApp.Features.AiVisualizer.Entities;

public sealed class GeneratedPreview
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid VisualizerRequestId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public PreviewView View { get; set; } = PreviewView.Front;
    public string AiProvider { get; set; } = string.Empty;
    public string ImageSize { get; set; } = "1024x1536";
    public string Quality { get; set; } = "standard";
    public decimal GenerationCost { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public VisualizerRequest? VisualizerRequest { get; set; }
}
