using Ecommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Infrastructure.BackgroundJobs;

/// <summary>
/// Drains the outbox. In the scaffold it only logs; swap the handler body for a real
/// message bus / email sender without touching any producer.
/// </summary>
public sealed class OutboxPublisher(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxPublisher> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await PublishPendingAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Outbox publishing failed; will retry.");
            }
        }
    }

    private async Task PublishPendingAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var pending = await db.OutboxMessages
            .Where(message => message.ProcessedAt == null)
            .OrderBy(message => message.OccurredAt)
            .Take(20)
            .ToListAsync(cancellationToken);

        if (pending.Count == 0)
        {
            return;
        }

        foreach (var message in pending)
        {
            logger.LogInformation(
                "Publishing domain event {EventType} ({OutboxMessageId})",
                message.Type,
                message.Id);

            message.MarkProcessed(DateTimeOffset.UtcNow);
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
