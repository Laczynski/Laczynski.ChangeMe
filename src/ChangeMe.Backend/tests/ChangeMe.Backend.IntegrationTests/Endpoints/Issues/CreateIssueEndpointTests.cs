using System.Net;
using System.Net.Http.Json;
using ChangeMe.Backend.Domain.Aggregates.Issue.Enums;
using ChangeMe.Backend.Infrastructure.Persistence;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeMe.Backend.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class CreateIssueEndpointTests(BackendWebApplicationFactory factory)
{
  [Fact]
  public async Task PostIssues_WhenUserIsAuthenticated_ShouldCreateIssueWithWatcherAndAcceptanceCriteria()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    using var client = user.Client;

    var request = new
    {
      Title = "Issue created from integration test",
      Description = "Created through HTTP",
      Status = IssueStatus.NEW,
      Priority = IssuePriority.HIGH,
      AssignedToUserId = (Guid?)null,
      WatchAfterCreate = true,
      AcceptanceCriteria = new[]
      {
        new { Content = "First acceptance criterion" },
        new { Content = "Second acceptance criterion" }
      }
    };

    var response = await client.PostAsJsonAsync("/api/v1/issues", request, cancellationToken);

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);

    await using var scope = factory.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var issue = await dbContext.Issues
      .Include(x => x.Watchers)
      .Include(x => x.AcceptanceCriteria)
      .Include(x => x.HistoryEntries)
      .SingleOrDefaultAsync(x => x.Title == request.Title, cancellationToken);

    Assert.NotNull(issue);
    Assert.Equal(request.Description, issue.Description);
    Assert.Equal(IssuePriority.HIGH, issue.Priority);
    Assert.Equal(IssueStatus.NEW, issue.Status);
    Assert.Single(issue.Watchers, x => x.UserId == user.UserId);
    Assert.Equal(2, issue.AcceptanceCriteria.Count);
    Assert.Single(issue.HistoryEntries, x => x.EventType == IssueHistoryEventType.ISSUE_CREATED);
  }

  [Fact]
  public async Task PostIssues_WhenUserIsAnonymous_ShouldReturnUnauthorized()
  {
    var cancellationToken = TestContext.Current.CancellationToken;

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var response = await client.PostAsJsonAsync("/api/v1/issues", new
    {
      Title = "Unauthorized issue",
      Description = "Should fail",
      Status = IssueStatus.NEW,
      Priority = IssuePriority.HIGH,
      WatchAfterCreate = false
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
  }
}
