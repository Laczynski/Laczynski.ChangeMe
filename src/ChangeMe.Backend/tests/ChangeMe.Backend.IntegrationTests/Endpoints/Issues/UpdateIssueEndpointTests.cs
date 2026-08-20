using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ChangeMe.Backend.Domain.Aggregates.Issue.Enums;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Infrastructure.Persistence;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeMe.Backend.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class UpdateIssueEndpointTests(BackendWebApplicationFactory factory)
{
  [Fact]
  public async Task PutIssue_WhenRequestIsValid_ShouldUpdateIssueAndSynchronizeAcceptanceCriteria()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    using var client = await TestAuthHelper.CreateAuthenticatedClientAsync(factory, cancellationToken);

    var issueId = await IssueTestHelper.SeedIssueAsync(
      factory,
      "Initial title",
      "Initial description",
      IssuePriority.MEDIUM,
      ["Old criterion", "Remove me"],
      cancellationToken);

    await using var arrangeScope = factory.Services.CreateAsyncScope();
    var arrangeDb = arrangeScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var existingAcceptanceCriterionId = await arrangeDb.Issues
      .AsNoTracking()
      .Where(x => x.Id == issueId)
      .SelectMany(x => x.AcceptanceCriteria)
      .Select(x => x.Id)
      .FirstAsync(cancellationToken);

    var response = await client.PutAsJsonAsync($"/api/v1/issues/{issueId}", new
    {
      Id = issueId,
      Version = 0L,
      Title = "Updated title",
      Description = "Updated description",
      Status = IssueStatus.IN_PROGRESS,
      Priority = IssuePriority.CRITICAL,
      AssignedToUserId = (Guid?)null,
      AcceptanceCriteria = new object[]
      {
        new
        {
          Id = existingAcceptanceCriterionId,
          Content = "Updated acceptance criterion"
        },
        new
        {
          Content = "New acceptance criterion"
        }
      }
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    await using var assertScope = factory.Services.CreateAsyncScope();
    var assertDb = assertScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var issue = await assertDb.Issues
      .Include(x => x.AcceptanceCriteria)
      .Include(x => x.HistoryEntries)
      .SingleAsync(x => x.Id == issueId, cancellationToken);

    Assert.Equal("Updated title", issue.Title);
    Assert.Equal("Updated description", issue.Description);
    Assert.Equal(IssuePriority.CRITICAL, issue.Priority);
    Assert.Equal(IssueStatus.IN_PROGRESS, issue.Status);
    Assert.Equal(2, issue.AcceptanceCriteria.Count);
    Assert.Contains(issue.AcceptanceCriteria, x => x.Content == "Updated acceptance criterion");
    Assert.Contains(issue.AcceptanceCriteria, x => x.Content == "New acceptance criterion");
    Assert.DoesNotContain(issue.AcceptanceCriteria, x => x.Content == "Remove me");
    Assert.Contains(issue.HistoryEntries, x => x.EventType == IssueHistoryEventType.STATUS_CHANGED);
    Assert.Contains(issue.HistoryEntries, x => x.EventType == IssueHistoryEventType.PRIORITY_CHANGED);
    Assert.Contains(issue.HistoryEntries, x => x.EventType == IssueHistoryEventType.ACCEPTANCE_CRITERION_ADDED);
    Assert.Contains(issue.HistoryEntries, x => x.EventType == IssueHistoryEventType.ACCEPTANCE_CRITERION_UPDATED);
    Assert.Contains(issue.HistoryEntries, x => x.EventType == IssueHistoryEventType.ACCEPTANCE_CRITERION_REMOVED);
    Assert.Equal(1, issue.Version);
  }

  [Fact]
  public async Task PutIssue_WhenVersionIsStale_ShouldReturnConflict()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    using var client = await TestAuthHelper.CreateAuthenticatedClientAsync(factory, cancellationToken);

    var issueId = await IssueTestHelper.SeedIssueAsync(
      factory,
      "Concurrency title",
      "Concurrency description",
      IssuePriority.MEDIUM,
      null,
      cancellationToken);

    var staleResponse = await client.PutAsJsonAsync($"/api/v1/issues/{issueId}", new
    {
      Id = issueId,
      Version = 99L,
      Title = "Should not save",
      Description = "Concurrency description",
      Status = IssueStatus.NEW,
      Priority = IssuePriority.MEDIUM,
      AssignedToUserId = (Guid?)null,
      AcceptanceCriteria = Array.Empty<object>()
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.Conflict, staleResponse.StatusCode);

    var body = await staleResponse.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(ConcurrencyMessages.StaleVersion, body, StringComparison.Ordinal);

    await using var assertScope = factory.Services.CreateAsyncScope();
    var assertDb = assertScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var issue = await assertDb.Issues.AsNoTracking().SingleAsync(x => x.Id == issueId, cancellationToken);

    Assert.Equal("Concurrency title", issue.Title);
    Assert.Equal(0, issue.Version);
  }

  [Fact]
  public async Task PutIssue_WhenAssigneeChanges_ShouldExposeReadableAssigneeNamesInHistory()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var actor = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var assignee = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    using var client = actor.Client;

    var issueId = await IssueTestHelper.SeedIssueAsync(
      factory,
      "Issue with assignee change",
      "Initial description",
      IssuePriority.MEDIUM,
      null,
      cancellationToken,
      actorId: actor.UserId);

    var updateResponse = await client.PutAsJsonAsync($"/api/v1/issues/{issueId}", new
    {
      Id = issueId,
      Version = 0L,
      Title = "Issue with assignee change",
      Description = "Initial description",
      Status = IssueStatus.NEW,
      Priority = IssuePriority.MEDIUM,
      AssignedToUserId = assignee.UserId,
      AcceptanceCriteria = Array.Empty<object>()
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

    var historyResponse = await client.GetAsync($"/api/v1/issues/{issueId}/history", cancellationToken);
    var responseBody = await historyResponse.Content.ReadAsStringAsync(cancellationToken);

    Assert.Equal(HttpStatusCode.OK, historyResponse.StatusCode);

    using var document = JsonDocument.Parse(responseBody);
    var historyEntries = document.RootElement.GetProperty("value").GetProperty("items");

    var assigneeChangedEntry = historyEntries
      .EnumerateArray()
      .First(entry => entry.GetProperty("eventType").GetString() == IssueHistoryEventType.ASSIGNEE_CHANGED.ToString());

    Assert.Equal("Unassigned", assigneeChangedEntry.GetProperty("previousValue").GetString());
    var currentValue = assigneeChangedEntry.GetProperty("currentValue").GetString();
    Assert.Equal(UserDisplayFormat.DisplayLabel("Test", "User", assignee.Email), currentValue);
  }
}
