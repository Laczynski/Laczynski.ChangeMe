using ChangeMe.Backend.Domain.Aggregates.Issue;
using ChangeMe.Backend.Domain.Aggregates.Issue.Enums;
using ChangeMe.Backend.Infrastructure.Persistence;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeMe.Backend.IntegrationTests.Support;

internal static class IssueTestHelper
{
  public static async Task<Guid> SeedIssueAsync(
    BackendWebApplicationFactory factory,
    string title,
    string description,
    IssuePriority priority,
    string[]? acceptanceCriteria,
    CancellationToken cancellationToken,
    IssueStatus status = IssueStatus.NEW,
    Guid? assignedToUserId = null,
    Guid? actorId = null,
    bool addActorAsWatcher = false)
  {
    await using var scope = factory.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var effectiveActorId = actorId ?? Guid.CreateVersion7();

    var issueResult = Issue.Create(title, description, priority, status, assignedToUserId);
    var issue = issueResult.Value;
    issue.RecordCreation(effectiveActorId);

    foreach (var acceptanceCriterion in acceptanceCriteria ?? [])
    {
      issue.AddAcceptanceCriterion(acceptanceCriterion);
    }

    if (addActorAsWatcher)
      issue.StartWatching(effectiveActorId);

    ApplyAudit(issue, effectiveActorId);
    ApplyAudit(issue.HistoryEntries, effectiveActorId);
    ApplyAudit(issue.AcceptanceCriteria, effectiveActorId);
    ApplyAudit(issue.Comments, effectiveActorId);
    ApplyAudit(issue.Watchers, effectiveActorId);

    dbContext.Issues.Add(issue);
    await dbContext.SaveChangesAsync(cancellationToken);

    return issue.Id;
  }

  private static void ApplyAudit(Entity entity, Guid actorId)
  {
    entity.CreatedBy = actorId;
    entity.UpdatedBy = actorId;
  }

  private static void ApplyAudit(IEnumerable<Entity> entities, Guid actorId)
  {
    foreach (var entity in entities)
      ApplyAudit(entity, actorId);
  }
}
