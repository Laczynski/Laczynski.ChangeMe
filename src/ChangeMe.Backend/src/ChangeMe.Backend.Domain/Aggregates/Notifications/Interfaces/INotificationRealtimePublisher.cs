using ChangeMe.Backend.Domain.Aggregates.Notifications.Enums;

namespace ChangeMe.Backend.Domain.Aggregates.Notifications.Interfaces;

public interface INotificationRealtimePublisher
{
  Task PublishAsync(Guid userId, NotificationRealtimeMessage message, CancellationToken cancellationToken);
}

public class NotificationRealtimeMessage
{
  public Guid NotificationId { get; set; }
  public Guid IssueId { get; set; }
  public NotificationEventType EventType { get; set; }
  public string IssueTitle { get; set; } = string.Empty;
  public string Message { get; set; } = string.Empty;
  public string Link { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; }
}
