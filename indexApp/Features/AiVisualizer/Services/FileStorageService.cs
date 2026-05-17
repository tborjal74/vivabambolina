using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Options;

namespace indexApp.Features.AiVisualizer.Services;

public sealed class FileStorageService
{
    private readonly IWebHostEnvironment environment;
    private readonly AiVisualizerOptions options;

    public FileStorageService(IWebHostEnvironment environment, IOptions<AiVisualizerOptions> options)
    {
        this.environment = environment;
        this.options = options.Value;
    }

    public async Task<string> SaveUploadAsync(IBrowserFile file, string userId, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(file.Name);
        var safeExtension = string.IsNullOrWhiteSpace(extension) ? ".jpg" : extension.ToLowerInvariant();
        var relativeDirectory = Path.Combine("uploads", "visualizer", userId);
        var absoluteDirectory = Path.Combine(environment.WebRootPath, relativeDirectory);
        Directory.CreateDirectory(absoluteDirectory);

        var fileName = $"{Guid.NewGuid():N}{safeExtension}";
        var absolutePath = Path.Combine(absoluteDirectory, fileName);
        await using var output = File.Create(absolutePath);
        await file.OpenReadStream(options.MaxUploadBytes).CopyToAsync(output, cancellationToken);

        return "/" + Path.Combine(relativeDirectory, fileName).Replace('\\', '/');
    }

    public async Task<string> SaveUploadAsync(IFormFile file, string userId, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(file.FileName);
        var safeExtension = string.IsNullOrWhiteSpace(extension) ? ".jpg" : extension.ToLowerInvariant();
        var relativeDirectory = Path.Combine("uploads", "visualizer", userId);
        var absoluteDirectory = Path.Combine(environment.WebRootPath, relativeDirectory);
        Directory.CreateDirectory(absoluteDirectory);

        var fileName = $"{Guid.NewGuid():N}{safeExtension}";
        var absolutePath = Path.Combine(absoluteDirectory, fileName);
        await using var output = File.Create(absolutePath);
        await file.CopyToAsync(output, cancellationToken);

        return "/" + Path.Combine(relativeDirectory, fileName).Replace('\\', '/');
    }
}
