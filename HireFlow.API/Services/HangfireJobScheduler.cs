using Hangfire;

namespace HireFlow.API.Services;

/// <summary>
/// Hosted service that registers Hangfire recurring jobs when the application starts.
/// </summary>
public class HangfireJobScheduler : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // 1. Auto-Close Old Jobs - Runs daily at midnight
        RecurringJob.AddOrUpdate<JobBackgroundService>(
            "auto-close-old-jobs",
            service => service.CloseExpiredJobsAsync(),
            Cron.Daily);

        // 2. Cleanup Expired JWT Tokens - Runs monthly (on the 1st day of the month at midnight)
        RecurringJob.AddOrUpdate<JobBackgroundService>(
            "cleanup-expired-tokens",
            service => service.CleanupExpiredTokensAsync(),
            Cron.Monthly);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
