using System.Net.Http.Headers;
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
    private readonly FileStorageService fileStorageService;
    private readonly AiVisualizerOptions options;
    private readonly ILogger<OpenAiGownVisualizationService> logger;

    public OpenAiGownVisualizationService(
        HttpClient httpClient,
        FileStorageService fileStorageService,
        IOptions<AiVisualizerOptions> options,
        ILogger<OpenAiGownVisualizationService> logger)
    {
        this.httpClient = httpClient;
        this.fileStorageService = fileStorageService;
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

        var inputImagePath = fileStorageService.ResolveUploadedPhotoPath(request.CustomerImageFileName)
            ?? throw new InvalidOperationException("Upload a valid customer photo before generating an AI preview.");
        using var httpRequest = CreateEditRequest(request, inputImagePath);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);

        using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning(
                "OpenAI image generation failed with status {StatusCode}: {Response}",
                response.StatusCode,
                responseContent);

            throw new InvalidOperationException(response.StatusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized => "OpenAI rejected the API key. Create a new project API key and update the server secret.",
                System.Net.HttpStatusCode.Forbidden => "OpenAI denied image generation for this project. Check model access and organization verification.",
                System.Net.HttpStatusCode.TooManyRequests => "OpenAI usage limits or billing limits were reached. Check project billing and rate limits.",
                System.Net.HttpStatusCode.BadRequest => "OpenAI rejected the image request. Check the configured image model, size, and quality.",
                _ => $"OpenAI image generation failed with status code {(int)response.StatusCode}."
            });
        }

        var payload = JsonSerializer.Deserialize<OpenAiImageGenerationResponse>(responseContent, SerializerOptions);
        var imageBase64 = payload?.Data.FirstOrDefault()?.Base64Json;
        if (string.IsNullOrWhiteSpace(imageBase64))
        {
            throw new InvalidOperationException("OpenAI image generation response did not include image data.");
        }

        var imageBytes = Convert.FromBase64String(imageBase64);
        var imageUrl = await fileStorageService.SaveGeneratedImageAsync(imageBytes, cancellationToken);

        return new GownVisualizationResult(
            JobId: $"openai_{Guid.NewGuid():N}",
            Status: "Ready",
            PreviewImageUrl: new Uri(imageUrl, UriKind.Relative),
            Provider: "OpenAI",
            CreatedAt: DateTimeOffset.UtcNow,
            Warnings: []);
    }

    private HttpRequestMessage CreateEditRequest(GownVisualizationRequest request, string imagePath)
    {
        var endpoint = string.IsNullOrWhiteSpace(options.EditEndpoint)
            ? "https://api.openai.com/v1/images/edits"
            : options.EditEndpoint;

        var content = new MultipartFormDataContent
        {
            { new StringContent(options.Model), "model" },
            { new StringContent(request.Prompt ?? request.ProductTitle), "prompt" },
            { new StringContent(NormalizeSize(request.ImageSize ?? options.ImageSize)), "size" },
            { new StringContent(NormalizeQuality(request.Quality ?? options.ImageQuality)), "quality" },
            { new StringContent("png"), "output_format" },
            { new StringContent("high"), "input_fidelity" }
        };

        var imageContent = new StreamContent(File.OpenRead(imagePath));
        imageContent.Headers.ContentType = new MediaTypeHeaderValue(request.CustomerImageContentType);
        content.Add(imageContent, "image", Path.GetFileName(imagePath));

        return new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = content
        };
    }

    private static string NormalizeSize(string? size)
    {
        return size is "1024x1024" or "1024x1536" or "1536x1024"
            ? size
            : "1024x1536";
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

    private sealed record OpenAiImageGenerationResponse(
        [property: JsonPropertyName("data")] IReadOnlyList<OpenAiImageData> Data);

    private sealed record OpenAiImageData(
        [property: JsonPropertyName("b64_json")] string? Base64Json,
        [property: JsonPropertyName("url")] string? Url,
        [property: JsonPropertyName("revised_prompt")] string? RevisedPrompt);
}
