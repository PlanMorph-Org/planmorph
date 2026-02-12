using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PlanMorph.Core.Entities;
using PlanMorph.Infrastructure.Data;

namespace PlanMorph.Api.Services;

public class RejectedApplicationsCleanupService : BackgroundService
{
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(24);
    private static readonly TimeSpan RetentionPeriod = TimeSpan.FromDays(30);
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RejectedApplicationsCleanupService> _logger;

    public RejectedApplicationsCleanupService(IServiceScopeFactory scopeFactory, ILogger<RejectedApplicationsCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clean up rejected applications.");
            }

            try
            {
                await Task.Delay(CleanupInterval, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                return;
            }
        }
    }

    private async Task CleanupAsync(CancellationToken stoppingToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var cutoffDate = DateTime.UtcNow.Subtract(RetentionPeriod);

        var candidates = await userManager.Users
            .Where(u => (u.Role == UserRole.Architect || u.Role == UserRole.Engineer)
                        && u.IsRejected
                        && !u.IsActive
                        && u.RejectedAt != null
                        && u.RejectedAt <= cutoffDate)
            .ToListAsync(stoppingToken);

        if (candidates.Count == 0)
        {
            return;
        }

        foreach (var candidate in candidates)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var hasDesigns = await dbContext.Designs.AnyAsync(d => d.ArchitectId == candidate.Id, stoppingToken);
            if (hasDesigns)
            {
                _logger.LogWarning("Skipped deleting rejected professional {UserId} because designs exist.", candidate.Id);
                continue;
            }

            var reviewLogs = dbContext.ProfessionalReviewLogs.Where(l => l.ProfessionalUserId == candidate.Id);
            dbContext.ProfessionalReviewLogs.RemoveRange(reviewLogs);
            await dbContext.SaveChangesAsync(stoppingToken);

            var result = await userManager.DeleteAsync(candidate);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Failed to delete rejected professional {UserId}: {Errors}", candidate.Id, result.Errors);
            }
        }
    }
}
