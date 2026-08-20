using ChangeMe.Backend.Domain.Aggregates.Issue;
using ChangeMe.Backend.Domain.Aggregates.Issue.Enums;
namespace ChangeMe.Backend.UseCases.Issues.Utils;

public static class IssuesUtils
{
  public const string AssignedUserDoesNotExistMessage = "assigned user does not exist";

  public static async Task<Result> ValidateAssigneeExistsAsync(
    ApplicationDbContext context,
    Guid? assignedToUserId,
    string propertyName,
    CancellationToken cancellationToken)
  {
    if (!assignedToUserId.HasValue)
      return Result.Success();

    var assigneeExists = await context.Users
      .AsNoTracking()
      .AnyAsync(u => u.Id == assignedToUserId.Value, cancellationToken);

    if (!assigneeExists)
      return Result.Invalid(new ValidationError(propertyName, AssignedUserDoesNotExistMessage));

    return Result.Success();
  }

  public static HashSet<Guid> CollectRelatedUserIds(Issue issue)
  {
    var userIds = new HashSet<Guid> { issue.CreatedBy };

    if (issue.AssignedToUserId.HasValue)
      userIds.Add(issue.AssignedToUserId.Value);

    foreach (var assigneeHistoryEntry in issue.HistoryEntries.Where(h => h.EventType == IssueHistoryEventType.ASSIGNEE_CHANGED))
    {
      if (Guid.TryParse(assigneeHistoryEntry.PreviousValue, out var previousAssigneeId))
        userIds.Add(previousAssigneeId);

      if (Guid.TryParse(assigneeHistoryEntry.CurrentValue, out var currentAssigneeId))
        userIds.Add(currentAssigneeId);
    }

    foreach (var comment in issue.Comments)
      userIds.Add(comment.CreatedBy);

    foreach (var attachment in issue.Attachments)
      userIds.Add(attachment.CreatedBy);

    foreach (var historyEntry in issue.HistoryEntries)
      userIds.Add(historyEntry.ActorUserId);

    return userIds;
  }

  public static async Task<Dictionary<Guid, string>> GetUserDisplayNameLookupAsync(
    ApplicationDbContext context,
    IEnumerable<Guid> userIds,
    CancellationToken cancellationToken)
  {
    var distinctUserIds = userIds.Distinct().ToList();
    if (distinctUserIds.Count == 0)
      return [];

    var users = await context.Users
      .AsNoTracking()
      .Where(u => distinctUserIds.Contains(u.Id))
      .ToListAsync(cancellationToken);

    return users.ToDictionary(u => u.Id, u => u.DisplayLabel);
  }

  public static bool IsNotificationEligible(IssueHistoryEventType eventType) =>
    eventType is
      IssueHistoryEventType.STATUS_CHANGED or
      IssueHistoryEventType.PRIORITY_CHANGED or
      IssueHistoryEventType.ASSIGNEE_CHANGED or
      IssueHistoryEventType.TITLE_CHANGED or
      IssueHistoryEventType.DESCRIPTION_CHANGED or
      IssueHistoryEventType.ACCEPTANCE_CRITERION_ADDED or
      IssueHistoryEventType.ACCEPTANCE_CRITERION_UPDATED or
      IssueHistoryEventType.ACCEPTANCE_CRITERION_REMOVED or
      IssueHistoryEventType.ATTACHMENT_ADDED or
      IssueHistoryEventType.ATTACHMENT_REMOVED;
}
