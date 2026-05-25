using System.Security.Claims;
using indexApp.Features.AiVisualizer.Models;
using indexApp.Features.AiVisualizer.Services;
using Microsoft.AspNetCore.Mvc;

namespace indexApp.Features.AiVisualizer;

public static class VisualizerEndpoints
{
    public static IEndpointRouteBuilder MapVisualizerEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/visualizer").RequireAuthorization();

        group.MapGet("/styles", async (VisualizerService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.GetStylesAsync(cancellationToken)));

        group.MapGet("/fabrics", async (VisualizerService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.GetFabricsAsync(cancellationToken)));

        group.MapPost("/request", async (
            [FromBody] CreateVisualizerRequestDto request,
            VisualizerService service,
            ClaimsPrincipal user,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.CreateRequestAsync(GetUserId(user), request, cancellationToken: cancellationToken)));

        group.MapPut("/request/{id:guid}/style", async (
            Guid id,
            [FromBody] UpdateStyleRequest request,
            VisualizerService service,
            ClaimsPrincipal user,
            CancellationToken cancellationToken) =>
        {
            var result = await service.UpdateStyleAsync(GetUserId(user), id, request.DressStyleId, cancellationToken);
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        group.MapPut("/request/{id:guid}/fabric", async (
            Guid id,
            [FromBody] UpdateFabricRequest request,
            VisualizerService service,
            ClaimsPrincipal user,
            CancellationToken cancellationToken) =>
        {
            var result = await service.UpdateFabricAsync(GetUserId(user), id, request.FabricId, request.FabricColorId, cancellationToken);
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        group.MapPut("/request/{id:guid}/measurements", async (
            Guid id,
            [FromBody] MeasurementDto request,
            VisualizerService service,
            ClaimsPrincipal user,
            CancellationToken cancellationToken) =>
        {
            var result = await service.UpdateMeasurementsAsync(GetUserId(user), id, request, cancellationToken);
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        group.MapPost("/request/{id:guid}/upload-photo", async (
            Guid id,
            IFormFile file,
            FileStorageService storage,
            VisualizerService service,
            ClaimsPrincipal user,
            CancellationToken cancellationToken) =>
        {
            var userId = GetUserId(user);
            var photoUrl = await storage.SaveUploadAsync(file, userId, cancellationToken);
            var result = await service.UpdatePhotoAsync(userId, id, photoUrl, cancellationToken);
            return result is null ? Results.NotFound() : Results.Ok(result);
        }).DisableAntiforgery();

        group.MapPost("/request/{id:guid}/generate-basic-preview", async (
            Guid id,
            VisualizerService service,
            ClaimsPrincipal user,
            CancellationToken cancellationToken) =>
        {
            var result = await service.SaveBasicPreviewAsync(GetUserId(user), id, cancellationToken);
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        group.MapPost("/request/{id:guid}/generate-ai-preview", async (
            Guid id,
            [FromBody] GenerateAiPreviewRequest request,
            VisualizerService service,
            ClaimsPrincipal user,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.GenerateAiPreviewAsync(
                GetUserId(user),
                request with { VisualizerRequestId = id },
                cancellationToken)));

        group.MapGet("/request/{id:guid}", async (
            Guid id,
            VisualizerService service,
            ClaimsPrincipal user,
            CancellationToken cancellationToken) =>
        {
            var result = await service.GetRequestAsync(GetUserId(user), id, cancellationToken);
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        group.MapGet("/request/{id:guid}/uploaded-photo", ViewUploadedPhotoAsync);
        group.MapGet("/request/{id:guid}/preview-image", ViewPreviewImageAsync);
        group.MapGet("/request/{id:guid}/download-preview", DownloadPreviewAsync);
        group.MapDelete("/request/{id:guid}/media", async (
            Guid id,
            VisualizerService service,
            ClaimsPrincipal user,
            CancellationToken cancellationToken) =>
        {
            var removed = await service.DeletePrivateMediaAsync(GetUserId(user), id, cancellationToken);
            return removed ? Results.NoContent() : Results.NotFound();
        });

        group.MapGet("/history", async (
            VisualizerService service,
            ClaimsPrincipal user,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.GetHistoryAsync(GetUserId(user), cancellationToken)));

        return endpoints;
    }

    private static async Task<IResult> DownloadPreviewAsync(
        Guid id,
        VisualizerService service,
        FileStorageService storage,
        IWebHostEnvironment environment,
        ClaimsPrincipal user,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var storageReference = await service.GetLatestGeneratedPreviewImageUrlAsync(GetUserId(user), id, cancellationToken);
        var imagePath = ResolvePreviewPath(storageReference, storage, environment);
        if (imagePath is null)
        {
            return Results.NotFound();
        }

        AddPrivateImageHeaders(httpContext);
        return Results.File(
            imagePath,
            GetContentType(imagePath),
            $"viva-bambolina-ai-preview-{id:N}{Path.GetExtension(imagePath).ToLowerInvariant()}");
    }

    private static async Task<IResult> ViewUploadedPhotoAsync(
        Guid id,
        VisualizerService service,
        FileStorageService storage,
        ClaimsPrincipal user,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var storageReference = await service.GetUploadedPhotoStorageReferenceAsync(GetUserId(user), id, cancellationToken);
        var imagePath = storage.ResolveUploadedPhotoPath(storageReference);
        return ReturnPrivateImage(imagePath, httpContext);
    }

    private static async Task<IResult> ViewPreviewImageAsync(
        Guid id,
        VisualizerService service,
        FileStorageService storage,
        IWebHostEnvironment environment,
        ClaimsPrincipal user,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var storageReference = await service.GetLatestGeneratedPreviewImageUrlAsync(GetUserId(user), id, cancellationToken);
        var imagePath = ResolvePreviewPath(storageReference, storage, environment);
        return ReturnPrivateImage(imagePath, httpContext);
    }

    private static string? ResolvePreviewPath(
        string? storageReference,
        FileStorageService storage,
        IWebHostEnvironment environment)
    {
        var generatedImagePath = storage.ResolveGeneratedPreviewPath(storageReference);
        if (generatedImagePath is not null)
        {
            return generatedImagePath;
        }

        return storageReference == "/images/visualizer-placeholder.svg"
            ? Path.Combine(environment.WebRootPath, "images", "visualizer-placeholder.svg")
            : null;
    }

    private static IResult ReturnPrivateImage(string? imagePath, HttpContext httpContext)
    {
        if (imagePath is null)
        {
            return Results.NotFound();
        }

        AddPrivateImageHeaders(httpContext);
        return Results.File(imagePath, GetContentType(imagePath));
    }

    private static string GetContentType(string imagePath)
    {
        return Path.GetExtension(imagePath).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".webp" => "image/webp",
            ".svg" => "image/svg+xml",
            _ => "image/png"
        };
    }

    private static void AddPrivateImageHeaders(HttpContext httpContext)
    {
        httpContext.Response.Headers.CacheControl = "private, no-store";
        httpContext.Response.Headers.Pragma = "no-cache";
    }

    private static string GetUserId(ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Authenticated user id was not found.");
    }

    private sealed record UpdateStyleRequest(Guid DressStyleId);

    private sealed record UpdateFabricRequest(Guid FabricId, Guid FabricColorId);
}
