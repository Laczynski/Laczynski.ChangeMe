using ChangeMe.Backend.Domain.Aggregates.Notifications.Enums;

namespace ChangeMe.Backend.Domain.Aggregates.Notifications;

public class Notification : Entity, IAggregateRoot
{
  private Notification() { }

  public Guid RecipientUserId { get; private set; }
  public Guid IssueId { get; private set; }
  public Guid IssueHistoryEntryId { get; private set; }
  public NotificationEventType EventType { get; private set; }
  public string IssueTitle { get; private set; } = string.Empty;
  public string Message { get; private set; } = string.Empty;
  public string Link { get; private set; } = string.Empty;
  public bool IsRead { get; private set; }
  public DateTime? ReadAt { get; private set; }
  public DateTime? EmailSentAt { get; private set; }

  public static Result<Notification> Create(
    Guid recipientUserId,
    Guid issueId,
    Guid issueHistoryEntryId,
    NotificationEventType eventType,
    string issueTitle,
    string message,
    string link)
  {
    var validationErrors = new List<ValidationError>();

    if (recipientUserId == Guid.Empty)
      validationErrors.Add(new ValidationError(nameof(RecipientUserId), "cannot be empty"));
    if (issueId == Guid.Empty)
      validationErrors.Add(new ValidationError(nameof(IssueId), "cannot be empty"));
    if (issueHistoryEntryId == Guid.Empty)
      validationErrors.Add(new ValidationError(nameof(IssueHistoryEntryId), "cannot be empty"));
    if (!Enum.IsDefined(eventType))
      validationErrors.Add(new ValidationError(nameof(EventType), "invalid event type"));
    if (string.IsNullOrWhiteSpace(issueTitle))
      validationErrors.Add(new ValidationError(nameof(IssueTitle), "cannot be empty"));
    else if (issueTitle.Trim().Length > NotificationConstraints.ISSUE_TITLE_MAX_LENGTH)
      validationErrors.Add(new ValidationError(nameof(IssueTitle), $"cannot be longer than {NotificationConstraints.ISSUE_TITLE_MAX_LENGTH} characters"));
    if (string.IsNullOrWhiteSpace(message))
      validationErrors.Add(new ValidationError(nameof(Message), "cannot be empty"));
    else if (message.Trim().Length > NotificationConstraints.MESSAGE_MAX_LENGTH)
      validationErrors.Add(new ValidationError(nameof(Message), $"cannot be longer than {NotificationConstraints.MESSAGE_MAX_LENGTH} characters"));
    if (string.IsNullOrWhiteSpace(link))
      validationErrors.Add(new ValidationError(nameof(Link), "cannot be empty"));
    else if (link.Trim().Length > NotificationConstraints.LINK_MAX_LENGTH)
      validationErrors.Add(new ValidationError(nameof(Link), $"cannot be longer than {NotificationConstraints.LINK_MAX_LENGTH} characters"));

    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    return Result.Success(new Notification
    {
      RecipientUserId = recipientUserId,
      IssueId = issueId,
      IssueHistoryEntryId = issueHistoryEntryId,
      EventType = eventType,
      IssueTitle = issueTitle.Trim(),
      Message = message.Trim(),
      Link = link.Trim(),
      IsRead = false,
    });
  }

  public void MarkAsRead()
  {
    if (IsRead)
      return;

    IsRead = true;
    ReadAt = DateTime.UtcNow;
  }

  public void MarkEmailSent()
  {
    EmailSentAt = DateTime.UtcNow;
  }
}

public static class NotificationConstraints
{
  public const int ISSUE_TITLE_MAX_LENGTH = 255;
  public const int MESSAGE_MAX_LENGTH = 1000;
  public const int LINK_MAX_LENGTH = 500;
}
