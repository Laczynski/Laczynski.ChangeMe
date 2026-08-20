using System.Net;
using ChangeMe.Backend.Domain.Aggregates.Issue.Enums;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ChangeMe.Backend.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class GetIssueByIdEndpointTests(BackendWebApplicationFactory factory)
{
  [Fact]
  public async Task GetIssueById_WhenIssueExists_ShouldReturnOk()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    using var client = await TestAuthHelper.CreateAuthenticatedClientAsync(factory, cancellationToken);

    var issueId = await IssueTestHelper.SeedIssueAsync(
      factory,
      "Issue for get by id",
      "Issue description",
      IssuePriority.HIGH,
      ["Acceptance criterion A"],
      cancellationToken);

    var response = await client.GetAsync($"/api/v1/issues/{issueId}", cancellationToken);
    var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.Contains("Issue for get by id", responseBody, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("Acceptance criterion A", responseBody, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public async Task GetIssueById_WhenIssueDoesNotExistAndUserIsAuthenticated_ShouldReturnNotFound()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    using var client = await TestAuthHelper.CreateAuthenticatedClientAsync(factory, cancellationToken);

    var response = await client.GetAsync($"/api/v1/issues/{Guid.NewGuid()}", cancellationToken);

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task GetIssueById_WhenUserIsAnonymous_ShouldReturnUnauthorized()
  {
    var cancellationToken = TestContext.Current.CancellationToken;

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var response = await client.GetAsync($"/api/v1/issues/{Guid.NewGuid()}", cancellationToken);

    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
  }
}
