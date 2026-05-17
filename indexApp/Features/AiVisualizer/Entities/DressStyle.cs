namespace indexApp.Features.AiVisualizer.Entities;

public sealed class DressStyle
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public string Silhouette { get; set; } = string.Empty;
    public string Neckline { get; set; } = string.Empty;
    public string SleeveType { get; set; } = string.Empty;
    public string DressLength { get; set; } = string.Empty;
    public string? TemplateImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
