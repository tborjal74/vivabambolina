using indexApp.Features.AiVisualizer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace indexApp.Features.AiVisualizer.Services;

public sealed class UsageLimitService
{
    private readonly AiVisualizerDbContext dbContext;
    private readonly AiVisualizerOptions options;

    public UsageLimitService(AiVisualizerDbContext dbContext, IOptions<AiVisualizerOptions> options)
    {
        this.dbContext = dbContext;
        this.options = options.Value;
    }

    public async Task<bool> CanGenerateAsync(string userId, CancellationToken cancellationToken = default)
    {
        var since = DateTimeOffset.UtcNow.Date;
        var count = await dbContext.AiUsageLogs
            .CountAsync(log => log.UserId == userId && log.Operation == "generate-ai-preview" && log.CreatedAt >= since, cancellationToken);

        return count < options.MaxAiPreviewsPerUserPerDay;
    }
}
