using System.Net;
using ChangeMe.Backend.Domain.Aggregates.Issue.Enums;
using ChangeMe.Backend.Infrastructure.Persistence;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeMe.Backend.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class DeleteIssueEndpointTests(BackendWebApplicationFactory factory)
{
  [Fact]
  public async Task DeleteIssue_WhenIssueExists_ShouldDeleteIssue()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    using var client = await TestAuthHelper.CreateAuthenticatedClientAsync(factory, cancellationToken);

    var issueId = await IssueTestHelper.SeedIssueAsync(
      factory,
      "Issue to delete",
      "Delete me",
      IssuePriority.LOW,
      null,
      cancellationToken);

    var response = await client.DeleteAsync($"/api/v1/issues/{issueId}", cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    await using var scope = factory.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var issueExists = dbContext.Issues.Any(x => x.Id == issueId);

    Assert.False(issueExists);
  }
}
