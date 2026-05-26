using indexApp.Features.AiVisualizer.Entities;
using Microsoft.EntityFrameworkCore;

namespace indexApp.Features.AiVisualizer.Data;

public sealed class AiVisualizerDbContext : DbContext
{
    public AiVisualizerDbContext(DbContextOptions<AiVisualizerDbContext> options)
        : base(options)
    {
    }

    public DbSet<DressStyle> DressStyles => Set<DressStyle>();

    public DbSet<Fabric> Fabrics => Set<Fabric>();

    public DbSet<FabricColor> FabricColors => Set<FabricColor>();

    public DbSet<CustomerMeasurement> CustomerMeasurements => Set<CustomerMeasurement>();

    public DbSet<VisualizerRequest> VisualizerRequests => Set<VisualizerRequest>();

    public DbSet<GeneratedPreview> GeneratedPreviews => Set<GeneratedPreview>();

    public DbSet<AiUsageLog> AiUsageLogs => Set<AiUsageLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DressStyle>(entity =>
        {
            entity.ToTable("dress_styles");
            entity.HasKey(style => style.Id);
            entity.Property(style => style.Id).HasColumnName("id");
            entity.Property(style => style.Name).HasColumnName("name").HasMaxLength(120).IsRequired();
            entity.Property(style => style.Description).HasColumnName("description").HasMaxLength(600).IsRequired();
            entity.Property(style => style.BasePrice).HasColumnName("base_price").HasPrecision(10, 2);
            entity.Property(style => style.Silhouette).HasColumnName("silhouette").HasMaxLength(80).IsRequired();
            entity.Property(style => style.Neckline).HasColumnName("neckline").HasMaxLength(80).IsRequired();
            entity.Property(style => style.SleeveType).HasColumnName("sleeve_type").HasMaxLength(80).IsRequired();
            entity.Property(style => style.DressLength).HasColumnName("dress_length").HasMaxLength(80).IsRequired();
            entity.Property(style => style.TemplateImageUrl).HasColumnName("template_image_url").HasMaxLength(500);
            entity.Property(style => style.IsActive).HasColumnName("is_active");
            entity.Property(style => style.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<Fabric>(entity =>
        {
            entity.ToTable("fabrics");
            entity.HasKey(fabric => fabric.Id);
            entity.Property(fabric => fabric.Id).HasColumnName("id");
            entity.Property(fabric => fabric.Name).HasColumnName("name").HasMaxLength(120).IsRequired();
            entity.Property(fabric => fabric.Description).HasColumnName("description").HasMaxLength(600).IsRequired();
            entity.Property(fabric => fabric.TextureImageUrl).HasColumnName("texture_image_url").HasMaxLength(500);
            entity.Property(fabric => fabric.MaterialType).HasColumnName("material_type").HasMaxLength(80).IsRequired();
            entity.Property(fabric => fabric.IsActive).HasColumnName("is_active");
            entity.Property(fabric => fabric.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<FabricColor>(entity =>
        {
            entity.ToTable("fabric_colors");
            entity.HasKey(color => color.Id);
            entity.Property(color => color.Id).HasColumnName("id");
            entity.Property(color => color.FabricId).HasColumnName("fabric_id");
            entity.Property(color => color.ColorName).HasColumnName("color_name").HasMaxLength(120).IsRequired();
            entity.Property(color => color.HexCode).HasColumnName("hex_code").HasMaxLength(16).IsRequired();
            entity.Property(color => color.SwatchImageUrl).HasColumnName("swatch_image_url").HasMaxLength(500);
            entity.Property(color => color.IsActive).HasColumnName("is_active");

            entity.HasOne(color => color.Fabric)
                .WithMany(fabric => fabric.Colors)
                .HasForeignKey(color => color.FabricId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CustomerMeasurement>(entity =>
        {
            entity.ToTable("customer_measurements");
            entity.HasKey(measurement => measurement.Id);
            entity.Property(measurement => measurement.Id).HasColumnName("id");
            entity.Property(measurement => measurement.UserId).HasColumnName("user_id").HasMaxLength(120).IsRequired();
            entity.Property(measurement => measurement.Size).HasColumnName("size").HasMaxLength(8).IsRequired();
            entity.Property(measurement => measurement.Chest).HasColumnName("chest").HasPrecision(6, 2);
            entity.Property(measurement => measurement.Waist).HasColumnName("waist").HasPrecision(6, 2);
            entity.Property(measurement => measurement.ShoulderToShoulder).HasColumnName("shoulder_to_shoulder").HasPrecision(6, 2);
            entity.Property(measurement => measurement.ShoulderToWaistSeam).HasColumnName("shoulder_to_waist_seam").HasPrecision(6, 2);
            entity.Property(measurement => measurement.ShoulderToHem).HasColumnName("shoulder_to_hem").HasPrecision(6, 2);
            entity.Property(measurement => measurement.SleeveLength).HasColumnName("sleeve_length").HasPrecision(6, 2);
            entity.Property(measurement => measurement.Bicep).HasColumnName("bicep").HasPrecision(6, 2);
            entity.Property(measurement => measurement.EndOfSleeve).HasColumnName("end_of_sleeve").HasPrecision(6, 2);
            entity.Property(measurement => measurement.ShoulderToSideNeck).HasColumnName("shoulder_to_side_neck").HasPrecision(6, 2);
            entity.Property(measurement => measurement.ShoulderToNeckOpening).HasColumnName("shoulder_to_neck_opening").HasPrecision(6, 2);
            entity.Property(measurement => measurement.MockNeckHeight).HasColumnName("mock_neck_height").HasPrecision(6, 2);
            entity.Property(measurement => measurement.MockNeckCircumference).HasColumnName("mock_neck_circumference").HasPrecision(6, 2);
            entity.Property(measurement => measurement.UnderarmToSideWaistSeam).HasColumnName("underarm_to_side_waist_seam").HasPrecision(6, 2);
            entity.Property(measurement => measurement.CenterNeckToWaistPoint).HasColumnName("center_neck_to_waist_point").HasPrecision(6, 2);
            entity.Property(measurement => measurement.CuffHeight).HasColumnName("cuff_height").HasPrecision(6, 2);
            entity.Property(measurement => measurement.SkirtShape).HasColumnName("skirt_shape").HasMaxLength(80).IsRequired();
            entity.Property(measurement => measurement.HasHorsehairEdgeHem).HasColumnName("has_horsehair_edge_hem");
            entity.Property(measurement => measurement.NecklineShape).HasColumnName("neckline_shape").HasMaxLength(80).IsRequired();
            entity.Property(measurement => measurement.SleeveShape).HasColumnName("sleeve_shape").HasMaxLength(80).IsRequired();
            entity.Property(measurement => measurement.HasBuiltInPuffy).HasColumnName("has_built_in_puffy");
            entity.Property(measurement => measurement.CreatedAt).HasColumnName("created_at");
            entity.Property(measurement => measurement.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<VisualizerRequest>(entity =>
        {
            entity.ToTable("visualizer_requests");
            entity.HasKey(request => request.Id);
            entity.Property(request => request.Id).HasColumnName("id");
            entity.Property(request => request.UserId).HasColumnName("user_id").HasMaxLength(120).IsRequired();
            entity.Property(request => request.DressStyleId).HasColumnName("dress_style_id");
            entity.Property(request => request.FabricId).HasColumnName("fabric_id");
            entity.Property(request => request.FabricColorId).HasColumnName("fabric_color_id");
            entity.Property(request => request.MeasurementId).HasColumnName("measurement_id");
            entity.Property(request => request.UploadedPhotoUrl).HasColumnName("uploaded_photo_url").HasMaxLength(500);
            entity.Property(request => request.BasicPreviewUrl).HasColumnName("basic_preview_url").HasMaxLength(500);
            entity.Property(request => request.PromptUsed).HasColumnName("prompt_used").HasMaxLength(3000);
            entity.Property(request => request.Status).HasColumnName("status").HasConversion<int>();
            entity.Property(request => request.ErrorMessage).HasColumnName("error_message").HasMaxLength(800);
            entity.Property(request => request.CreatedAt).HasColumnName("created_at");
            entity.Property(request => request.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(request => request.DressStyle).WithMany().HasForeignKey(request => request.DressStyleId);
            entity.HasOne(request => request.Fabric).WithMany().HasForeignKey(request => request.FabricId);
            entity.HasOne(request => request.FabricColor).WithMany().HasForeignKey(request => request.FabricColorId);
            entity.HasOne(request => request.Measurement).WithMany().HasForeignKey(request => request.MeasurementId);
        });

        modelBuilder.Entity<GeneratedPreview>(entity =>
        {
            entity.ToTable("generated_previews");
            entity.HasKey(preview => preview.Id);
            entity.Property(preview => preview.Id).HasColumnName("id");
            entity.Property(preview => preview.VisualizerRequestId).HasColumnName("visualizer_request_id");
            entity.Property(preview => preview.ImageUrl).HasColumnName("image_url").HasMaxLength(500).IsRequired();
            entity.Property(preview => preview.AiProvider).HasColumnName("ai_provider").HasMaxLength(80).IsRequired();
            entity.Property(preview => preview.ImageSize).HasColumnName("image_size").HasMaxLength(40).IsRequired();
            entity.Property(preview => preview.Quality).HasColumnName("quality").HasMaxLength(40).IsRequired();
            entity.Property(preview => preview.GenerationCost).HasColumnName("generation_cost").HasPrecision(10, 4);
            entity.Property(preview => preview.CreatedAt).HasColumnName("created_at");

            entity.HasOne(preview => preview.VisualizerRequest)
                .WithMany(request => request.GeneratedPreviews)
                .HasForeignKey(preview => preview.VisualizerRequestId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AiUsageLog>(entity =>
        {
            entity.ToTable("ai_usage_logs");
            entity.HasKey(log => log.Id);
            entity.Property(log => log.Id).HasColumnName("id");
            entity.Property(log => log.UserId).HasColumnName("user_id").HasMaxLength(120).IsRequired();
            entity.Property(log => log.VisualizerRequestId).HasColumnName("visualizer_request_id");
            entity.Property(log => log.Provider).HasColumnName("provider").HasMaxLength(80).IsRequired();
            entity.Property(log => log.Operation).HasColumnName("operation").HasMaxLength(80).IsRequired();
            entity.Property(log => log.EstimatedInputTokens).HasColumnName("estimated_input_tokens");
            entity.Property(log => log.EstimatedOutputTokens).HasColumnName("estimated_output_tokens");
            entity.Property(log => log.CostEstimate).HasColumnName("cost_estimate").HasPrecision(10, 4);
            entity.Property(log => log.CreatedAt).HasColumnName("created_at");
        });
    }
}
