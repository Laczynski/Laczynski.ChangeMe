using ChangeMe.Backend.UseCases.Notifications.Dtos;
using ChangeMe.Backend.UseCases.Notifications.Services;

namespace ChangeMe.Backend.UseCases.Notifications;

public record MarkNotificationAsReadCommand(Guid NotificationId) : ICommand<NotificationDto>;

public class MarkNotificationAsReadHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  NotificationRetentionPolicy retentionPolicy) : ICommandHandler<MarkNotificationAsReadCommand, NotificationDto>
{
  public async ValueTask<Result<NotificationDto>> Handle(MarkNotificationAsReadCommand command, CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid currentUserId)
      return Result.Unauthorized();

    var notification = await retentionPolicy.ApplyActiveFilter(context.Notifications)
      .FirstOrDefaultAsync(n => n.Id == command.NotificationId && n.RecipientUserId == currentUserId, cancellationToken);

    if (notification is null)
      return Result.NotFound();

    notification.MarkAsRead();
    await context.SaveChangesAsync(cancellationToken);

    return Result.Success(new NotificationDto
    {
      Id = notification.Id,
      IssueId = notification.IssueId,
      EventType = notification.EventType,
      IssueTitle = notification.IssueTitle,
      Message = notification.Message,
      Link = notification.Link,
      CreatedAt = notification.CreatedAt,
      IsRead = notification.IsRead,
      ReadAt = notification.ReadAt,
    });
  }
}
