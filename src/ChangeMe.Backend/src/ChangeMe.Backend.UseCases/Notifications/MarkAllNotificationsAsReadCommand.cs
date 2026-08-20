using ChangeMe.Backend.UseCases.Notifications.Services;

namespace ChangeMe.Backend.UseCases.Notifications;

public sealed record MarkAllNotificationsAsReadCommand() : ICommand<bool>;

public class MarkAllNotificationsAsReadHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  NotificationRetentionPolicy retentionPolicy) : ICommandHandler<MarkAllNotificationsAsReadCommand, bool>
{
  public async ValueTask<Result<bool>> Handle(
    MarkAllNotificationsAsReadCommand command,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid currentUserId)
      return Result.Unauthorized();

    var notifications = await retentionPolicy.ApplyActiveFilter(context.Notifications)
      .Where(n => n.RecipientUserId == currentUserId && !n.IsRead)
      .ToListAsync(cancellationToken);

    foreach (var notification in notifications)
      notification.MarkAsRead();

    await context.SaveChangesAsync(cancellationToken);

    return Result.Success(true);
  }
}
