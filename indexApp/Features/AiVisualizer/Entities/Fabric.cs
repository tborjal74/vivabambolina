namespace indexApp.Features.AiVisualizer.Entities;

public sealed class Fabric
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? TextureImageUrl { get; set; }
    public string MaterialType { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<FabricColor> Colors { get; set; } = [];
}
