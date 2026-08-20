using ChangeMe.Backend.Domain.Aggregates.Issue;
using ChangeMe.Backend.Domain.Aggregates.Issue.Enums;
using ChangeMe.Backend.UseCases.Issues.Dtos;

namespace ChangeMe.Backend.UseCases.Issues.Utils;

public static class IssueMappingExtensions
{
  private const string UnassignedHistoryValue = "Unassigned";

  public static IssueDetailsDto ToDetailsDto(
    this Issue issue,
    IReadOnlyDictionary<Guid, string> userLookup,
    Guid? currentUserId)
  {
    return new IssueDetailsDto
    {
      Id = issue.Id,
      Title = issue.Title,
      Description = issue.Description,
      Status = issue.Status,
      Priority = issue.Priority,
      CreatedBy = issue.CreatedBy,
      CreatedByName = userLookup.GetValueOrDefault(issue.CreatedBy),
      AssignedToUserId = issue.AssignedToUserId,
      AssignedToUserName = issue.AssignedToUserId.HasValue ? userLookup.GetValueOrDefault(issue.AssignedToUserId.Value) : null,
      CreatedAt = issue.CreatedAt,
      UpdatedAt = issue.UpdatedAt,
      Version = issue.Version,
      LastActivityAt = issue.LastActivityAt,
      IsWatchedByCurrentUser = currentUserId.HasValue && issue.Watchers.Any(w => w.UserId == currentUserId.Value),
      WatchersCount = issue.Watchers.Count,
      AcceptanceCriteria = issue.AcceptanceCriteria
        .OrderBy(c => c.CreatedAt)
        .Select(c => new AcceptanceCriterionDto
        {
          Id = c.Id,
          Content = c.Content,
          CreatedAt = c.CreatedAt,
          CreatedBy = c.CreatedBy,
        })
        .ToList(),
    };
  }

  public static IssueHistoryEntryDto ToHistoryEntryDto(
    this IssueHistoryEntryDto entry,
    IReadOnlyDictionary<Guid, string> userLookup) =>
    new()
    {
      Id = entry.Id,
      EventType = entry.EventType,
      ActorUserId = entry.ActorUserId,
      ActorName = userLookup.GetValueOrDefault(entry.ActorUserId),
      Summary = entry.Summary,
      PreviousValue = FormatHistoryValue(entry.EventType, entry.PreviousValue, userLookup),
      CurrentValue = FormatHistoryValue(entry.EventType, entry.CurrentValue, userLookup),
      CreatedAt = entry.CreatedAt
    };

  public static IEnumerable<Guid> CollectHistoryRelatedUserIds(IEnumerable<IssueHistoryEntryDto> entries)
  {
    var userIds = new HashSet<Guid>();

    foreach (var entry in entries)
    {
      userIds.Add(entry.ActorUserId);

      if (entry.EventType != IssueHistoryEventType.ASSIGNEE_CHANGED)
        continue;

      if (Guid.TryParse(entry.PreviousValue, out var previousAssigneeId))
        userIds.Add(previousAssigneeId);

      if (Guid.TryParse(entry.CurrentValue, out var currentAssigneeId))
        userIds.Add(currentAssigneeId);
    }

    return userIds;
  }

  private static string? FormatHistoryValue(
    IssueHistoryEventType eventType,
    string? value,
    IReadOnlyDictionary<Guid, string> userLookup)
  {
    if (eventType != IssueHistoryEventType.ASSIGNEE_CHANGED)
      return value;

    if (string.IsNullOrWhiteSpace(value))
      return UnassignedHistoryValue;

    return Guid.TryParse(value, out var userId)
      ? userLookup.GetValueOrDefault(userId, value)
      : value;
  }
}
