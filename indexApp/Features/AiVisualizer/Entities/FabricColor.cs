namespace indexApp.Features.AiVisualizer.Entities;

public sealed class FabricColor
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FabricId { get; set; }
    public string ColorName { get; set; } = string.Empty;
    public string HexCode { get; set; } = "#ffffff";
    public string? SwatchImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public Fabric? Fabric { get; set; }
}
