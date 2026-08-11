using DataGrid.Abstractions;
using Template.Backend.Domain.Aggregates.Notifications.Enums;

namespace Template.Backend.UseCases.Notifications.Dtos;

public class NotificationDto
{
  public Guid Id { get; set; }
  public Guid IssueId { get; set; }
  public NotificationEventType EventType { get; set; }
  public string IssueTitle { get; set; } = string.Empty;
  public string Message { get; set; } = string.Empty;
  public string Link { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; }
  public bool IsRead { get; set; }
  public DateTime? ReadAt { get; set; }
}

public class NotificationListDto
{
  public int UnreadCount { get; set; }
  public GridResult<NotificationDto> Page { get; set; } = new();
}
