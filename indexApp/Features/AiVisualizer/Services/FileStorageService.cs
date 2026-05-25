using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace indexApp.Features.AiVisualizer.Services;

public sealed class FileStorageService
{
    private const string UploadCategory = "uploads";
    private const string GeneratedCategory = "generated";

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    private readonly AiVisualizerOptions options;
    private readonly string storageRoot;
    private readonly string legacyPublicStorageRoot;

    public FileStorageService(IWebHostEnvironment environment, IOptions<AiVisualizerOptions> options)
    {
        this.options = options.Value;
        storageRoot = Path.GetFullPath(Path.Combine(environment.ContentRootPath, this.options.StoragePath));
        legacyPublicStorageRoot = Path.GetFullPath(Path.Combine(environment.WebRootPath, "uploads", "visualizer"));

        var webRoot = Path.GetFullPath(environment.WebRootPath);
        if (storageRoot.Equals(webRoot, StringComparison.OrdinalIgnoreCase) ||
            storageRoot.StartsWith(webRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("AiVisualizer:StoragePath must be outside wwwroot so uploaded and generated images are not public files.");
        }
    }

    public async Task<string> SaveUploadAsync(IBrowserFile file, string userId, CancellationToken cancellationToken = default)
    {
        var safeExtension = ValidateUpload(file.Name, file.ContentType, file.Size);
        var relativeDirectory = Path.Combine(UploadCategory, GetUserDirectoryName(userId));
        var absoluteDirectory = Path.Combine(storageRoot, relativeDirectory);
        Directory.CreateDirectory(absoluteDirectory);

        var fileName = $"{Guid.NewGuid():N}{safeExtension}";
        var absolutePath = Path.Combine(absoluteDirectory, fileName);
        await using var output = File.Create(absolutePath);
        await file.OpenReadStream(options.MaxUploadBytes).CopyToAsync(output, cancellationToken);

        return ToStorageReference(relativeDirectory, fileName);
    }

    public async Task<string> SaveUploadAsync(IFormFile file, string userId, CancellationToken cancellationToken = default)
    {
        var safeExtension = ValidateUpload(file.FileName, file.ContentType, file.Length);
        var relativeDirectory = Path.Combine(UploadCategory, GetUserDirectoryName(userId));
        var absoluteDirectory = Path.Combine(storageRoot, relativeDirectory);
        Directory.CreateDirectory(absoluteDirectory);

        var fileName = $"{Guid.NewGuid():N}{safeExtension}";
        var absolutePath = Path.Combine(absoluteDirectory, fileName);
        await using var output = File.Create(absolutePath);
        await file.CopyToAsync(output, cancellationToken);

        return ToStorageReference(relativeDirectory, fileName);
    }

    public async Task<string> SaveGeneratedImageAsync(byte[] imageBytes, CancellationToken cancellationToken = default)
    {
        var absoluteDirectory = Path.Combine(storageRoot, GeneratedCategory);
        Directory.CreateDirectory(absoluteDirectory);

        var fileName = $"{Guid.NewGuid():N}.png";
        var absolutePath = Path.Combine(absoluteDirectory, fileName);
        await File.WriteAllBytesAsync(absolutePath, imageBytes, cancellationToken);

        return ToStorageReference(GeneratedCategory, fileName);
    }

    public string? ResolveUploadedPhotoPath(string? storageReference)
    {
        return ResolveStoredFile(storageReference, UploadCategory);
    }

    public string? ResolveGeneratedPreviewPath(string? storageReference)
    {
        return ResolveStoredFile(storageReference, GeneratedCategory);
    }

    public Task<LegacyMediaCopy?> CopyLegacyUploadedPhotoToPrivateStorageAsync(
        string? publicUrl,
        CancellationToken cancellationToken = default)
    {
        return CopyLegacyFileToPrivateStorageAsync(publicUrl, UploadCategory, allowGeneratedSubdirectory: false, cancellationToken);
    }

    public Task<LegacyMediaCopy?> CopyLegacyGeneratedPreviewToPrivateStorageAsync(
        string? publicUrl,
        CancellationToken cancellationToken = default)
    {
        return CopyLegacyFileToPrivateStorageAsync(publicUrl, GeneratedCategory, allowGeneratedSubdirectory: true, cancellationToken);
    }

    public void DeleteLegacyPublicFile(string absolutePath)
    {
        var validatedPath = Path.GetFullPath(absolutePath);
        if (validatedPath.StartsWith(legacyPublicStorageRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) &&
            File.Exists(validatedPath))
        {
            File.Delete(validatedPath);
        }
    }

    public void DeletePrivateMedia(string? storageReference)
    {
        var absolutePath = ResolveUploadedPhotoPath(storageReference) ?? ResolveGeneratedPreviewPath(storageReference);
        if (absolutePath is not null)
        {
            File.Delete(absolutePath);
        }
    }

    public void DeleteUnreferencedExpiredPrivateMedia(
        IReadOnlySet<string> retainedReferences,
        DateTime expirationCutoffUtc)
    {
        if (!Directory.Exists(storageRoot))
        {
            return;
        }

        foreach (var category in new[] { UploadCategory, GeneratedCategory })
        {
            var directory = Path.Combine(storageRoot, category);
            if (!Directory.Exists(directory))
            {
                continue;
            }

            foreach (var path in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
            {
                var reference = Path.GetRelativePath(storageRoot, path).Replace('\\', '/');
                if (!retainedReferences.Contains(reference) &&
                    File.GetLastWriteTimeUtc(path) <= expirationCutoffUtc)
                {
                    File.Delete(path);
                }
            }
        }
    }

    private string ValidateUpload(string fileName, string contentType, long length)
    {
        var extension = Path.GetExtension(fileName);
        if (!AllowedExtensions.Contains(extension) ||
            !options.AllowedImageContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Please upload a JPG, PNG, or WEBP image.");
        }

        if (length > options.MaxUploadBytes)
        {
            throw new InvalidOperationException("Please upload an image that is 10MB or smaller.");
        }

        return extension.ToLowerInvariant();
    }

    private string? ResolveStoredFile(string? storageReference, string expectedCategory)
    {
        if (string.IsNullOrWhiteSpace(storageReference) ||
            storageReference.StartsWith("/", StringComparison.Ordinal))
        {
            return null;
        }

        var relativePath = storageReference.Replace('/', Path.DirectorySeparatorChar);
        var allowedDirectory = Path.GetFullPath(Path.Combine(storageRoot, expectedCategory));
        var absolutePath = Path.GetFullPath(Path.Combine(storageRoot, relativePath));

        if (!absolutePath.StartsWith(allowedDirectory + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
            !File.Exists(absolutePath))
        {
            return null;
        }

        return absolutePath;
    }

    private async Task<LegacyMediaCopy?> CopyLegacyFileToPrivateStorageAsync(
        string? publicUrl,
        string destinationCategory,
        bool allowGeneratedSubdirectory,
        CancellationToken cancellationToken)
    {
        const string legacyPrefix = "/uploads/visualizer/";
        if (string.IsNullOrWhiteSpace(publicUrl) ||
            !publicUrl.StartsWith(legacyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var relativeLegacyPath = publicUrl[legacyPrefix.Length..].Replace('/', Path.DirectorySeparatorChar);
        var isGeneratedPreview = relativeLegacyPath.StartsWith("generated" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
        if (isGeneratedPreview != allowGeneratedSubdirectory)
        {
            return null;
        }

        var sourcePath = Path.GetFullPath(Path.Combine(legacyPublicStorageRoot, relativeLegacyPath));
        if (!sourcePath.StartsWith(legacyPublicStorageRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
            !File.Exists(sourcePath))
        {
            return null;
        }

        var extension = Path.GetExtension(sourcePath).ToLowerInvariant();
        var destinationDirectory = Path.Combine(storageRoot, destinationCategory);
        Directory.CreateDirectory(destinationDirectory);

        var destinationReference = ToStorageReference(destinationCategory, $"{Guid.NewGuid():N}{extension}");
        var destinationPath = Path.Combine(storageRoot, destinationReference.Replace('/', Path.DirectorySeparatorChar));
        await using var sourceStream = File.OpenRead(sourcePath);
        await using var destinationStream = File.Create(destinationPath);
        await sourceStream.CopyToAsync(destinationStream, cancellationToken);

        return new LegacyMediaCopy(destinationReference, sourcePath);
    }

    private static string GetUserDirectoryName(string userId)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(userId))).ToLowerInvariant();
    }

    private static string ToStorageReference(string relativeDirectory, string fileName)
    {
        return Path.Combine(relativeDirectory, fileName).Replace('\\', '/');
    }

    public sealed record LegacyMediaCopy(string StorageReference, string LegacyAbsolutePath);
}
