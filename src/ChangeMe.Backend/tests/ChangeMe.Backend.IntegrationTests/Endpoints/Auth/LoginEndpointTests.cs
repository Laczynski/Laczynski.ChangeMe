using System.Net;
using System.Net.Http.Json;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ChangeMe.Backend.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class LoginEndpointTests(BackendWebApplicationFactory factory)
{
  [Fact]
  public async Task PostLogin_WhenCredentialsAreValid_ShouldReturnSuccessStatusCode()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var email = $"login-{Guid.NewGuid():N}@example.com";
    const string password = TestAuthHelper.DefaultUserPassword;

    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var userRoleId = await RolesTestHelper.GetRoleIdByNameAsync(factory, "User", cancellationToken);

    await admin.Client.PostAsJsonAsync("/api/v1/users", new
    {
      FirstName = "Login",
      LastName = "User",
      Email = email,
      Password = password,
      RoleIds = new[] { userRoleId }
    }, cancellationToken);

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
    {
      Email = email,
      Password = password
    }, cancellationToken);

    var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.Contains("token", responseBody, StringComparison.OrdinalIgnoreCase);
    Assert.Contains(email, responseBody, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public async Task PostLogin_WhenPasswordIsInvalid_ShouldReturnUnauthorized()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var email = $"login-invalid-{Guid.NewGuid():N}@example.com";
    const string password = TestAuthHelper.DefaultUserPassword;

    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var userRoleId = await RolesTestHelper.GetRoleIdByNameAsync(factory, "User", cancellationToken);

    await admin.Client.PostAsJsonAsync("/api/v1/users", new
    {
      FirstName = "Login",
      LastName = "Invalid",
      Email = email,
      Password = password,
      RoleIds = new[] { userRoleId }
    }, cancellationToken);

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
    {
      Email = email,
      Password = "WrongPass123!"
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
  }

  [Fact]
  public async Task PostLogin_WhenPasswordIsTooShort_ShouldReturnBadRequest()
  {
    var cancellationToken = TestContext.Current.CancellationToken;

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
    {
      Email = "user@example.com",
      Password = "short"
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }
}
