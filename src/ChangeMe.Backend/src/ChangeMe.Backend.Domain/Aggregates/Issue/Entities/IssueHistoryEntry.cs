using ChangeMe.Backend.Domain.Aggregates.Issue.Enums;

namespace ChangeMe.Backend.Domain.Aggregates.Issue.Entities;

public class IssueHistoryEntry : Entity
{
  private IssueHistoryEntry() { }

  public Guid IssueId { get; private set; }
  public IssueHistoryEventType EventType { get; private set; }
  public Guid ActorUserId { get; private set; }
  public string Summary { get; private set; } = string.Empty;
  public string? PreviousValue { get; private set; }
  public string? CurrentValue { get; private set; }
  public Guid? RelatedCommentId { get; private set; }

  public static Result<IssueHistoryEntry> Create(
    Guid issueId,
    IssueHistoryEventType eventType,
    Guid actorUserId,
    string summary,
    string? previousValue = null,
    string? currentValue = null,
    Guid? relatedCommentId = null)
  {
    var validationErrors = new List<ValidationError>();

    if (issueId == Guid.Empty)
      validationErrors.Add(new ValidationError(nameof(IssueId), "cannot be empty"));

    if (actorUserId == Guid.Empty)
      validationErrors.Add(new ValidationError(nameof(ActorUserId), "cannot be empty"));

    if (!Enum.IsDefined(eventType))
      validationErrors.Add(new ValidationError(nameof(EventType), "invalid event type"));

    if (string.IsNullOrWhiteSpace(summary))
      validationErrors.Add(new ValidationError(nameof(Summary), "cannot be empty"));
    else if (summary.Trim().Length > IssueHistoryConstraints.SUMMARY_MAX_LENGTH)
      validationErrors.Add(new ValidationError(nameof(Summary), $"cannot be longer than {IssueHistoryConstraints.SUMMARY_MAX_LENGTH} characters"));

    if (previousValue?.Length > IssueHistoryConstraints.VALUE_MAX_LENGTH)
      validationErrors.Add(new ValidationError(nameof(PreviousValue), $"cannot be longer than {IssueHistoryConstraints.VALUE_MAX_LENGTH} characters"));

    if (currentValue?.Length > IssueHistoryConstraints.VALUE_MAX_LENGTH)
      validationErrors.Add(new ValidationError(nameof(CurrentValue), $"cannot be longer than {IssueHistoryConstraints.VALUE_MAX_LENGTH} characters"));

    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    return Result.Success(new IssueHistoryEntry
    {
      IssueId = issueId,
      EventType = eventType,
      ActorUserId = actorUserId,
      Summary = summary.Trim(),
      PreviousValue = previousValue,
      CurrentValue = currentValue,
      RelatedCommentId = relatedCommentId,
    });
  }
}

public static class IssueHistoryConstraints
{
  public const int SUMMARY_MAX_LENGTH = 500;
  public const int VALUE_MAX_LENGTH = 2000;
}
