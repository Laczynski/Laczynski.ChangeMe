using System.Net;
using System.Net.Http.Json;
using ChangeMe.Backend.Domain.Aggregates.Issue.Enums;
using ChangeMe.Backend.Infrastructure.Persistence;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeMe.Backend.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class AddIssueCommentEndpointTests(BackendWebApplicationFactory factory)
{
  [Fact]
  public async Task PostIssueComment_WhenRequestIsValid_ShouldPersistCommentWithoutHistoryEntry()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    using var client = await TestAuthHelper.CreateAuthenticatedClientAsync(factory, cancellationToken);

    var issueId = await IssueTestHelper.SeedIssueAsync(
      factory,
      "Issue with comments",
      "Issue description",
      IssuePriority.HIGH,
      null,
      cancellationToken);

    var response = await client.PostAsJsonAsync($"/api/v1/issues/{issueId}/comments", new
    {
      IssueId = issueId,
      Content = "Comment added from integration test"
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    await using var scope = factory.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var issue = await dbContext.Issues
      .Include(i => i.Comments)
      .SingleAsync(i => i.Id == issueId, cancellationToken);

    Assert.Single(issue.Comments);
    Assert.Contains(issue.Comments, c => c.Content == "Comment added from integration test");
  }
}
