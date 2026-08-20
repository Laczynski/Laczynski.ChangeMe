using System.Net;
using System.Net.Http.Json;
using ChangeMe.Backend.Domain.Aggregates.Issue.Enums;
using ChangeMe.Backend.Infrastructure.Persistence;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeMe.Backend.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class WatchIssueEndpointTests(BackendWebApplicationFactory factory)
{
  [Fact]
  public async Task PostAndDeleteWatch_ShouldToggleWatcherForCurrentUser()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    using var client = user.Client;

    var issueId = await IssueTestHelper.SeedIssueAsync(
      factory,
      "Watchable issue",
      "Issue description",
      IssuePriority.MEDIUM,
      null,
      cancellationToken);

    var watchResponse = await client.PostAsJsonAsync($"/api/v1/issues/{issueId}/watch", new
    {
      IssueId = issueId
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.OK, watchResponse.StatusCode);

    var unwatchResponse = await client.DeleteAsync($"/api/v1/issues/{issueId}/watch", cancellationToken);

    Assert.Equal(HttpStatusCode.OK, unwatchResponse.StatusCode);

    await using var scope = factory.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var watcherCount = dbContext.IssueWatchers.Count(x => x.IssueId == issueId && x.UserId == user.UserId);

    Assert.Equal(0, watcherCount);
  }
}
