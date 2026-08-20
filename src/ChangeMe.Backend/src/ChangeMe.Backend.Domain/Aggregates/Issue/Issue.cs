using ChangeMe.Backend.Domain.Aggregates.Issue.Entities;
using ChangeMe.Backend.Domain.Aggregates.Issue.Enums;

namespace ChangeMe.Backend.Domain.Aggregates.Issue;

public class Issue : Entity, IAggregateRoot
{
  private readonly List<IssueAcceptanceCriterion> acceptanceCriteria = new();
  private readonly List<IssueComment> comments = new();
  private readonly List<IssueAttachment> attachments = new();
  private readonly List<IssueHistoryEntry> historyEntries = new();
  private readonly List<IssueWatcher> watchers = new();

  private Issue() { }

  public string Title { get; private set; } = string.Empty;
  public string Description { get; private set; } = string.Empty;
  public IssueStatus Status { get; private set; } = IssueStatus.NEW;
  public IssuePriority Priority { get; private set; } = IssuePriority.MEDIUM;
  public Guid? AssignedToUserId { get; private set; }
  public DateTime LastActivityAt { get; private set; }

  public IReadOnlyCollection<IssueAcceptanceCriterion> AcceptanceCriteria => acceptanceCriteria.AsReadOnly();
  public IReadOnlyCollection<IssueComment> Comments => comments.AsReadOnly();
  public IReadOnlyCollection<IssueAttachment> Attachments => attachments.AsReadOnly();
  public IReadOnlyCollection<IssueHistoryEntry> HistoryEntries => historyEntries.AsReadOnly();
  public IReadOnlyCollection<IssueWatcher> Watchers => watchers.AsReadOnly();

  public static Result<Issue> Create(
    string title,
    string description,
    IssuePriority priority = IssuePriority.MEDIUM,
    IssueStatus status = IssueStatus.NEW,
    Guid? assignedToUserId = null)
  {
    var validationErrors = Validate(title, description, priority, status);
    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    var issue = new Issue
    {
      Title = title.Trim(),
      Description = description.Trim(),
      Priority = priority,
      Status = status,
      AssignedToUserId = assignedToUserId,
      LastActivityAt = DateTime.UtcNow,
    };

    return Result.Success(issue);
  }

  public Result<Issue> RecordCreation(Guid actorUserId)
  {
    var historyResult = AddHistoryEntry(
      IssueHistoryEventType.ISSUE_CREATED,
      actorUserId,
      $"Issue '{Title}' was created.");

    if (!historyResult.IsSuccess)
      return historyResult.Map();

    return Result.Success(this);
  }

  public Result<Issue> UpdateDetails(
    string title,
    string description,
    IssuePriority priority,
    IssueStatus status,
    Guid? assignedToUserId,
    Guid actorUserId)
  {
    var validationErrors = Validate(title, description, priority, status);
    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    var normalizedTitle = title.Trim();
    var normalizedDescription = description.Trim();
    var hadChanges = false;

    var titleChangeResult = TryApplyDetailChange(
      !string.Equals(Title, normalizedTitle, StringComparison.Ordinal),
      IssueHistoryEventType.TITLE_CHANGED,
      actorUserId,
      "Issue title changed.",
      Title,
      normalizedTitle,
      () => Title = normalizedTitle);
    if (!titleChangeResult.IsSuccess)
      return titleChangeResult.Map();
    hadChanges |= titleChangeResult.Value;

    var descriptionChangeResult = TryApplyDetailChange(
      !string.Equals(Description, normalizedDescription, StringComparison.Ordinal),
      IssueHistoryEventType.DESCRIPTION_CHANGED,
      actorUserId,
      "Issue description changed.",
      Description,
      normalizedDescription,
      () => Description = normalizedDescription);
    if (!descriptionChangeResult.IsSuccess)
      return descriptionChangeResult.Map();
    hadChanges |= descriptionChangeResult.Value;

    var priorityChangeResult = TryApplyDetailChange(
      Priority != priority,
      IssueHistoryEventType.PRIORITY_CHANGED,
      actorUserId,
      "Issue priority changed.",
      Priority.ToString(),
      priority.ToString(),
      () => Priority = priority);
    if (!priorityChangeResult.IsSuccess)
      return priorityChangeResult.Map();
    hadChanges |= priorityChangeResult.Value;

    var statusChangeResult = TryApplyDetailChange(
      Status != status,
      IssueHistoryEventType.STATUS_CHANGED,
      actorUserId,
      "Issue status changed.",
      Status.ToString(),
      status.ToString(),
      () => Status = status);
    if (!statusChangeResult.IsSuccess)
      return statusChangeResult.Map();
    hadChanges |= statusChangeResult.Value;

    var assigneeChangeResult = TryApplyDetailChange(
      AssignedToUserId != assignedToUserId,
      IssueHistoryEventType.ASSIGNEE_CHANGED,
      actorUserId,
      "Issue assignee changed.",
      AssignedToUserId?.ToString(),
      assignedToUserId?.ToString(),
      () => AssignedToUserId = assignedToUserId);
    if (!assigneeChangeResult.IsSuccess)
      return assigneeChangeResult.Map();
    hadChanges |= assigneeChangeResult.Value;

    if (hadChanges)
      LastActivityAt = DateTime.UtcNow;

    return Result.Success(this);
  }

  public Result<IssueAcceptanceCriterion> AddAcceptanceCriterion(string content)
  {
    var acceptanceCriterion = IssueAcceptanceCriterion.Create(Id, content);
    if (!acceptanceCriterion.IsSuccess)
      return acceptanceCriterion.Map();

    acceptanceCriteria.Add(acceptanceCriterion.Value);
    return Result.Success(acceptanceCriterion.Value);
  }

  public Result<IssueAcceptanceCriterion> AddAcceptanceCriterion(string content, Guid actorUserId)
  {
    var acceptanceCriterionResult = AddAcceptanceCriterion(content);
    if (!acceptanceCriterionResult.IsSuccess)
      return acceptanceCriterionResult.Map();

    var historyResult = AddHistoryEntry(
      IssueHistoryEventType.ACCEPTANCE_CRITERION_ADDED,
      actorUserId,
      "Acceptance criterion added.",
      null,
      acceptanceCriterionResult.Value.Content);
    if (!historyResult.IsSuccess)
      return historyResult.Map();

    LastActivityAt = DateTime.UtcNow;
    return acceptanceCriterionResult;
  }

  public Result<IssueAcceptanceCriterion> UpdateAcceptanceCriterion(Guid acceptanceCriterionId, string content)
  {
    var acceptanceCriterion = acceptanceCriteria.FirstOrDefault(c => c.Id == acceptanceCriterionId);
    if (acceptanceCriterion is null)
      return Result.NotFound();

    return acceptanceCriterion.UpdateContent(content);
  }

  public Result<IssueAcceptanceCriterion> UpdateAcceptanceCriterion(Guid acceptanceCriterionId, string content, Guid actorUserId)
  {
    var acceptanceCriterion = acceptanceCriteria.FirstOrDefault(c => c.Id == acceptanceCriterionId);
    if (acceptanceCriterion is null)
      return Result.NotFound();

    var previousContent = acceptanceCriterion.Content;
    var updateResult = acceptanceCriterion.UpdateContent(content);
    if (!updateResult.IsSuccess)
      return updateResult.Map();

    if (!string.Equals(previousContent, updateResult.Value.Content, StringComparison.Ordinal))
    {
      var historyResult = AddHistoryEntry(
        IssueHistoryEventType.ACCEPTANCE_CRITERION_UPDATED,
        actorUserId,
        "Acceptance criterion updated.",
        previousContent,
        updateResult.Value.Content);
      if (!historyResult.IsSuccess)
        return historyResult.Map();

      LastActivityAt = DateTime.UtcNow;
    }

    return updateResult;
  }

  public Result<IssueAcceptanceCriterion> RemoveAcceptanceCriterion(Guid acceptanceCriterionId)
  {
    var acceptanceCriterion = acceptanceCriteria.FirstOrDefault(c => c.Id == acceptanceCriterionId);
    if (acceptanceCriterion is null)
      return Result.NotFound();

    acceptanceCriteria.Remove(acceptanceCriterion);
    return Result.Success(acceptanceCriterion);
  }

  public Result<IssueAcceptanceCriterion> RemoveAcceptanceCriterion(Guid acceptanceCriterionId, Guid actorUserId)
  {
    var acceptanceCriterion = acceptanceCriteria.FirstOrDefault(c => c.Id == acceptanceCriterionId);
    if (acceptanceCriterion is null)
      return Result.NotFound();

    var historyResult = AddHistoryEntry(
      IssueHistoryEventType.ACCEPTANCE_CRITERION_REMOVED,
      actorUserId,
      "Acceptance criterion removed.",
      acceptanceCriterion.Content,
      null);
    if (!historyResult.IsSuccess)
      return historyResult.Map();

    acceptanceCriteria.Remove(acceptanceCriterion);
    LastActivityAt = DateTime.UtcNow;
    return Result.Success(acceptanceCriterion);
  }

  public Result<IssueComment> AddComment(string content)
  {
    var commentResult = IssueComment.Create(Id, content);
    if (!commentResult.IsSuccess)
      return commentResult.Map();

    comments.Add(commentResult.Value);
    LastActivityAt = DateTime.UtcNow;

    return Result.Success(commentResult.Value);
  }

  public Result<IssueAttachment> AddAttachment(
    string originalFileName,
    string contentType,
    long sizeBytes,
    Guid actorUserId)
  {
    if (attachments.Count >= IssueConstraints.ATTACHMENT_MAX_ATTACHMENTS_PER_ISSUE)
      return Result.Invalid(new ValidationError(nameof(Attachments), $"cannot exceed {IssueConstraints.ATTACHMENT_MAX_ATTACHMENTS_PER_ISSUE} attachments per issue"));

    var attachmentResult = IssueAttachment.Create(
      Id,
      originalFileName,
      contentType,
      sizeBytes);

    if (!attachmentResult.IsSuccess)
      return attachmentResult.Map();

    var attachment = attachmentResult.Value;
    attachments.Add(attachment);

    var historyResult = AddHistoryEntry(
      IssueHistoryEventType.ATTACHMENT_ADDED,
      actorUserId,
      "Attachment added.",
      null,
      attachment.OriginalFileName);
    if (!historyResult.IsSuccess)
      return historyResult.Map();

    LastActivityAt = DateTime.UtcNow;
    return Result.Success(attachment);
  }

  public Result<IssueAttachment> RemoveAttachment(Guid attachmentId, Guid actorUserId)
  {
    var attachment = attachments.FirstOrDefault(a => a.Id == attachmentId);
    if (attachment is null)
      return Result.NotFound();

    if (attachment.CreatedBy != actorUserId)
      return Result.Forbidden();

    var historyResult = AddHistoryEntry(
      IssueHistoryEventType.ATTACHMENT_REMOVED,
      actorUserId,
      "Attachment removed.",
      attachment.OriginalFileName,
      null);
    if (!historyResult.IsSuccess)
      return historyResult.Map();

    attachments.Remove(attachment);
    LastActivityAt = DateTime.UtcNow;

    return Result.Success(attachment);
  }

  public Result<IssueWatcher> StartWatching(Guid userId)
  {
    if (watchers.Any(w => w.UserId == userId))
      return Result.Conflict("Issue is already watched by this user.");

    var watcher = IssueWatcher.Create(Id, userId);
    if (!watcher.IsSuccess)
      return watcher.Map();

    watchers.Add(watcher.Value);
    return Result.Success(watcher.Value);
  }

  public Result<IssueWatcher> StopWatching(Guid userId)
  {
    var watcher = watchers.FirstOrDefault(w => w.UserId == userId);
    if (watcher is null)
      return Result.NotFound();

    watchers.Remove(watcher);
    return Result.Success(watcher);
  }

  public bool IsWatchedBy(Guid userId) => watchers.Any(w => w.UserId == userId);

  private Result<bool> TryApplyDetailChange(
    bool hasChanged,
    IssueHistoryEventType eventType,
    Guid actorUserId,
    string summary,
    string? previousValue,
    string? currentValue,
    Action applyChange)
  {
    if (!hasChanged)
      return Result.Success(false);

    var historyResult = AddHistoryEntry(eventType, actorUserId, summary, previousValue, currentValue);
    if (!historyResult.IsSuccess)
      return historyResult.Map();

    applyChange();
    return Result.Success(true);
  }

  private Result<IssueHistoryEntry> AddHistoryEntry(
    IssueHistoryEventType eventType,
    Guid actorUserId,
    string summary,
    string? previousValue = null,
    string? currentValue = null,
    Guid? relatedCommentId = null)
  {
    var historyEntryResult = IssueHistoryEntry.Create(
      Id,
      eventType,
      actorUserId,
      summary,
      previousValue,
      currentValue,
      relatedCommentId);

    if (!historyEntryResult.IsSuccess)
      return historyEntryResult.Map();

    historyEntries.Add(historyEntryResult.Value);
    return Result.Success(historyEntryResult.Value);
  }

  private static List<ValidationError> Validate(
    string title,
    string description,
    IssuePriority priority,
    IssueStatus status)
  {
    var validationErrors = new List<ValidationError>();

    if (string.IsNullOrWhiteSpace(title))
      validationErrors.Add(new ValidationError(nameof(Title), "cannot be empty"));
    else
    {
      if (title.Trim().Length < IssueConstraints.TITLE_MIN_LENGTH)
        validationErrors.Add(new ValidationError(nameof(Title), $"must be at least {IssueConstraints.TITLE_MIN_LENGTH} characters long"));
      if (title.Trim().Length > IssueConstraints.TITLE_MAX_LENGTH)
        validationErrors.Add(new ValidationError(nameof(Title), $"cannot be longer than {IssueConstraints.TITLE_MAX_LENGTH} characters"));
    }

    if (string.IsNullOrWhiteSpace(description))
      validationErrors.Add(new ValidationError(nameof(Description), "cannot be empty"));
    else if (description.Trim().Length > IssueConstraints.DESCRIPTION_MAX_LENGTH)
      validationErrors.Add(new ValidationError(nameof(Description), $"cannot be longer than {IssueConstraints.DESCRIPTION_MAX_LENGTH} characters"));

    if (!Enum.IsDefined(priority))
      validationErrors.Add(new ValidationError(nameof(Priority), "invalid priority"));

    if (!Enum.IsDefined(status))
      validationErrors.Add(new ValidationError(nameof(Status), "invalid status"));

    return validationErrors;
  }
}

public static class IssueConstraints
{
  public const int TITLE_MIN_LENGTH = 3;
  public const int TITLE_MAX_LENGTH = 255;
  public const int DESCRIPTION_MAX_LENGTH = 2000;
  public const string STORAGE_CONTAINER = nameof(Issue);
  public const int ATTACHMENT_MAX_FILE_SIZE_BYTES = 5 * 1024 * 1024;
  public const int ATTACHMENT_MAX_ATTACHMENTS_PER_ISSUE = 10;
  public static readonly string[] ATTACHMENT_ALLOWED_EXTENSIONS =
  [
    ".pdf",
    ".png",
    ".jpg",
    ".jpeg",
    ".gif",
    ".txt",
    ".csv",
    ".docx",
    ".xlsx"
  ];
}
