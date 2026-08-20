using System.Net;
using ChangeMe.Backend.Domain.Aggregates.Issue.Enums;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support;
using DataGrid.Abstractions;
using DataGrid.Abstractions.Serialization;

namespace ChangeMe.Backend.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class GetAllIssuesEndpointTests(BackendWebApplicationFactory factory)
{
  [Fact]
  public async Task GetAllIssues_WhenIssuesExist_ShouldReturnOkWithSeededItems()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var marker = $"issues-marker-{Guid.NewGuid():N}";

    await IssueTestHelper.SeedIssueAsync(factory, $"{marker} Issue one", "First", IssuePriority.LOW, null, cancellationToken);
    await IssueTestHelper.SeedIssueAsync(factory, $"{marker} Issue two", "Second", IssuePriority.MEDIUM, null, cancellationToken);

    using var client = await TestAuthHelper.CreateAuthenticatedClientAsync(factory, cancellationToken);

    var grid = GridQueryJson.Serialize(new GridQuery { Take = 10, Search = marker });

    var response = await client.GetAsync(
      $"/api/v1/issues?grid={Uri.EscapeDataString(grid)}",
      cancellationToken);
    var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.Contains($"{marker} Issue one", responseBody, StringComparison.OrdinalIgnoreCase);
    Assert.Contains($"{marker} Issue two", responseBody, StringComparison.OrdinalIgnoreCase);
  }
}
