using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Template.Backend.Domain.Aggregates.Notifications;

namespace Template.Backend.UseCases.Notifications.Services;

public sealed class NotificationRetentionPolicy(
  IOptions<NotificationRetentionOptions> options,
  TimeProvider timeProvider)
{
  private readonly NotificationRetentionOptions retentionOptions = options.Value;

  public IQueryable<Notification> ApplyActiveFilter(IQueryable<Notification> queryable)
  {
    var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
    return queryable.Where(BuildActivePredicate(nowUtc));
  }

  public Expression<Func<Notification, bool>> BuildExpiredPredicate()
  {
    var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
    return BuildExpiredPredicate(nowUtc);
  }

  private Expression<Func<Notification, bool>> BuildActivePredicate(DateTime nowUtc)
  {
    var absoluteCutoff = nowUtc.AddDays(-retentionOptions.AbsoluteRetentionDays);
    var unreadCutoff = nowUtc.AddDays(-retentionOptions.UnreadRetentionDays);
    var readCutoff = nowUtc.AddDays(-retentionOptions.ReadRetentionDays);

    return notification =>
      notification.CreatedAt > absoluteCutoff
      && (
        (!notification.IsRead && notification.CreatedAt > unreadCutoff)
        || (notification.IsRead && (!notification.ReadAt.HasValue || notification.ReadAt > readCutoff))
      );
  }

  private Expression<Func<Notification, bool>> BuildExpiredPredicate(DateTime nowUtc)
  {
    var absoluteCutoff = nowUtc.AddDays(-retentionOptions.AbsoluteRetentionDays);
    var unreadCutoff = nowUtc.AddDays(-retentionOptions.UnreadRetentionDays);
    var readCutoff = nowUtc.AddDays(-retentionOptions.ReadRetentionDays);

    return notification =>
      notification.CreatedAt <= absoluteCutoff
      || (!notification.IsRead && notification.CreatedAt <= unreadCutoff)
      || (notification.IsRead && notification.ReadAt.HasValue && notification.ReadAt <= readCutoff);
  }
}

public sealed class NotificationRetentionCleanupJob(
  ApplicationDbContext context,
  NotificationRetentionPolicy retentionPolicy,
  ILogger<NotificationRetentionCleanupJob> logger)
{
  public Task ExecuteAsync(IJobCancellationToken jobCancellationToken)
  {
    jobCancellationToken.ThrowIfCancellationRequested();
    return ExecuteAsync(jobCancellationToken.ShutdownToken);
  }

  public async Task ExecuteAsync(CancellationToken cancellationToken)
  {
    var deletedCount = await context.Notifications
      .Where(retentionPolicy.BuildExpiredPredicate())
      .ExecuteDeleteAsync(cancellationToken);

    logger.LogInformation("Notification retention cleanup removed {DeletedCount} expired notifications", deletedCount);
  }
}
