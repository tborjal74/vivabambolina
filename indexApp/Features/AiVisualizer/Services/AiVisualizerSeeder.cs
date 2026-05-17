using indexApp.Features.AiVisualizer.Data;
using indexApp.Features.AiVisualizer.Entities;
using Microsoft.EntityFrameworkCore;

namespace indexApp.Features.AiVisualizer.Services;

public sealed class AiVisualizerSeeder
{
    private readonly AiVisualizerDbContext dbContext;

    public AiVisualizerSeeder(AiVisualizerDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await dbContext.DressStyles.AnyAsync(cancellationToken))
        {
            return;
        }

        var styles = new[]
        {
            new DressStyle { Name = "Embellished A-Line Gown", Description = "Balanced formal silhouette with a fitted bodice and flowing skirt.", BasePrice = 599, Silhouette = "A-line", Neckline = "sweetheart", SleeveType = "sleeveless", DressLength = "floor" },
            new DressStyle { Name = "Romantic Tulle Ball Gown", Description = "Full-volume skirt for a classic princess profile.", BasePrice = 749, Silhouette = "ball gown", Neckline = "off-shoulder", SleeveType = "cap sleeve", DressLength = "floor" },
            new DressStyle { Name = "Satin Mermaid Gown", Description = "Fitted through the hips with a dramatic lower flare.", BasePrice = 689, Silhouette = "mermaid", Neckline = "v-neck", SleeveType = "sleeveless", DressLength = "floor" },
            new DressStyle { Name = "Off-Shoulder Column Gown", Description = "Clean vertical shape with elegant shoulder framing.", BasePrice = 629, Silhouette = "column", Neckline = "off-shoulder", SleeveType = "short sleeve", DressLength = "floor" },
            new DressStyle { Name = "Long Sleeve Lace Gown", Description = "Modest lace overlay with fitted sleeves and soft skirt movement.", BasePrice = 799, Silhouette = "A-line", Neckline = "illusion", SleeveType = "long sleeve", DressLength = "floor" }
        };

        var sequinTulle = new Fabric { Name = "Sequin Tulle", Description = "Light tulle with reflective embellishment.", MaterialType = "tulle" };
        var organza = new Fabric { Name = "Soft Organza", Description = "Crisp sheer fabric with airy volume.", MaterialType = "organza" };
        var satin = new Fabric { Name = "Silk Satin", Description = "Smooth luminous fabric with elegant drape.", MaterialType = "satin" };

        sequinTulle.Colors.Add(new FabricColor { ColorName = "Blush Pink", HexCode = "#f1cfc3" });
        sequinTulle.Colors.Add(new FabricColor { ColorName = "Champagne", HexCode = "#e4bf95" });
        organza.Colors.Add(new FabricColor { ColorName = "Ivory", HexCode = "#fff8ef" });
        organza.Colors.Add(new FabricColor { ColorName = "Emerald", HexCode = "#0e623e" });
        satin.Colors.Add(new FabricColor { ColorName = "Midnight Navy", HexCode = "#13284d" });
        satin.Colors.Add(new FabricColor { ColorName = "Ruby", HexCode = "#bb1628" });
        satin.Colors.Add(new FabricColor { ColorName = "Onyx", HexCode = "#171717" });

        dbContext.DressStyles.AddRange(styles);
        dbContext.Fabrics.AddRange(sequinTulle, organza, satin);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
