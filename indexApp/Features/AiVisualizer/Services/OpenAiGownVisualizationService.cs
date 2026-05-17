using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using indexApp.Features.AiVisualizer.Models;
using Microsoft.Extensions.Options;

namespace indexApp.Features.AiVisualizer.Services;

public sealed class OpenAiGownVisualizationService : IGownVisualizationService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient httpClient;
    private readonly IWebHostEnvironment environment;
    private readonly AiVisualizerOptions options;
    private readonly ILogger<OpenAiGownVisualizationService> logger;

    public OpenAiGownVisualizationService(
        HttpClient httpClient,
        IWebHostEnvironment environment,
        IOptions<AiVisualizerOptions> options,
        ILogger<OpenAiGownVisualizationService> logger)
    {
        this.httpClient = httpClient;
        this.environment = environment;
        this.options = options.Value;
        this.logger = logger;
    }

    public async Task<GownVisualizationResult> CreatePreviewAsync(
        GownVisualizationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            throw new InvalidOperationException("OpenAI image generation is enabled, but AiVisualizer:ApiKey is not configured.");
        }

        var endpoint = string.IsNullOrWhiteSpace(options.Endpoint)
            ? "https://api.openai.com/v1/images/generations"
            : options.Endpoint;

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);

        var body = new OpenAiImageGenerationRequest(
            Model: options.Model,
            Prompt: request.Prompt ?? request.ProductTitle,
            Size: NormalizeSize(request.ImageSize ?? options.ImageSize),
            Quality: NormalizeQuality(request.Quality ?? options.ImageQuality),
            NumberOfImages: 1,
            OutputFormat: "png");

        httpRequest.Content = new StringContent(
            JsonSerializer.Serialize(body, SerializerOptions),
            Encoding.UTF8,
            "application/json");

        using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning(
                "OpenAI image generation failed with status {StatusCode}: {Response}",
                response.StatusCode,
                responseContent);

            throw new InvalidOperationException("OpenAI image generation failed. Check provider configuration and logs.");
        }

        var payload = JsonSerializer.Deserialize<OpenAiImageGenerationResponse>(responseContent, SerializerOptions);
        var imageBase64 = payload?.Data.FirstOrDefault()?.Base64Json;
        if (string.IsNullOrWhiteSpace(imageBase64))
        {
            throw new InvalidOperationException("OpenAI image generation response did not include image data.");
        }

        var imageBytes = Convert.FromBase64String(imageBase64);
        var imageUrl = await SaveGeneratedImageAsync(imageBytes, cancellationToken);

        return new GownVisualizationResult(
            JobId: $"openai_{Guid.NewGuid():N}",
            Status: "Ready",
            PreviewImageUrl: new Uri(imageUrl, UriKind.Relative),
            Provider: "OpenAI",
            CreatedAt: DateTimeOffset.UtcNow,
            Warnings: []);
    }

    private async Task<string> SaveGeneratedImageAsync(byte[] imageBytes, CancellationToken cancellationToken)
    {
        var relativeDirectory = Path.Combine("uploads", "visualizer", "generated");
        var absoluteDirectory = Path.Combine(environment.WebRootPath, relativeDirectory);
        Directory.CreateDirectory(absoluteDirectory);

        var fileName = $"{Guid.NewGuid():N}.png";
        var absolutePath = Path.Combine(absoluteDirectory, fileName);
        await File.WriteAllBytesAsync(absolutePath, imageBytes, cancellationToken);

        return "/" + Path.Combine(relativeDirectory, fileName).Replace('\\', '/');
    }

    private static string NormalizeSize(string? size)
    {
        return size is "1024x1024" or "1024x1536" or "1536x1024"
            ? size
            : "1024x1024";
    }

    private static string NormalizeQuality(string? quality)
    {
        return quality?.ToLowerInvariant() switch
        {
            "low" => "low",
            "medium" or "standard" => "medium",
            "high" or "hd" => "high",
            "auto" => "auto",
            _ => "medium"
        };
    }

    private sealed record OpenAiImageGenerationRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("prompt")] string Prompt,
        [property: JsonPropertyName("size")] string Size,
        [property: JsonPropertyName("quality")] string Quality,
        [property: JsonPropertyName("n")] int NumberOfImages,
        [property: JsonPropertyName("output_format")] string OutputFormat);

    private sealed record OpenAiImageGenerationResponse(
        [property: JsonPropertyName("data")] IReadOnlyList<OpenAiImageData> Data);

    private sealed record OpenAiImageData(
        [property: JsonPropertyName("b64_json")] string? Base64Json,
        [property: JsonPropertyName("url")] string? Url,
        [property: JsonPropertyName("revised_prompt")] string? RevisedPrompt);
}
