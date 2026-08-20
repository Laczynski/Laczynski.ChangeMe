using System.Net;
using System.Text.Json;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support;

namespace ChangeMe.Backend.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class GetAssignableUsersEndpointTests(BackendWebApplicationFactory factory)
{
  [Fact]
  public async Task GetAssignableUsers_WhenUserIsAuthenticated_ShouldReturnAssignableUsers()
  {
    var cancellationToken = TestContext.Current.CancellationToken;

    var firstUser = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);

    using var client = firstUser.Client;

    var response = await client.GetAsync("/api/v1/issues/assignable-users", cancellationToken);
    var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.Contains("Test User", responseBody, StringComparison.OrdinalIgnoreCase);

    using var document = JsonDocument.Parse(responseBody);
    var users = document.RootElement.GetProperty("value");
    Assert.True(users.GetArrayLength() >= 2);
  }

  [Fact]
  public async Task GetAssignableUsers_WhenUserIsAnonymous_ShouldReturnUnauthorized()
  {
    var cancellationToken = TestContext.Current.CancellationToken;

    using var client = factory.CreateClient();

    var response = await client.GetAsync("/api/v1/issues/assignable-users", cancellationToken);

    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
  }
}
