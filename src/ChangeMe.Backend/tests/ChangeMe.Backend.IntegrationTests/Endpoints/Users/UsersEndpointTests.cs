using System.Net;
using System.Net.Http.Json;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support;
using DataGrid.Abstractions;
using DataGrid.Abstractions.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeMe.Backend.IntegrationTests.Endpoints.Users;

[Collection(IntegrationTestCollection.Name)]
public sealed class UsersEndpointTests(BackendWebApplicationFactory factory)
{
  [Fact]
  public async Task GetUsers_WhenUserLacksPermission_ShouldReturnForbidden()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);

    var response = await user.Client.GetAsync("/api/v1/users?grid=%7B%22take%22%3A10%7D", cancellationToken);

    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
  }

  [Fact]
  public async Task GetUsers_WhenAdministrator_ShouldReturnOk()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);

    var grid = GridQueryJson.Serialize(new GridQuery { Take = 10 });

    var response = await admin.Client.GetAsync(
      $"/api/v1/users?grid={Uri.EscapeDataString(grid)}",
      cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  public async Task PostUsers_WhenAdministratorCreatesUser_ShouldReturnCreated()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);

    var userRoleId = await RolesTestHelper.GetRoleIdByNameAsync(factory, "User", cancellationToken);
    var email = $"managed-{Guid.NewGuid():N}@example.com";
    const string password = TestAuthHelper.DefaultUserPassword;

    var response = await admin.Client.PostAsJsonAsync("/api/v1/users", new
    {
      FirstName = "Managed",
      LastName = "User",
      Email = email,
      Password = password,
      RoleIds = new[] { userRoleId }
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);

    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(email, body, StringComparison.OrdinalIgnoreCase);

    await using var scope = factory.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ChangeMe.Backend.Infrastructure.Persistence.ApplicationDbContext>();
    var createdUser = await dbContext.Users.SingleAsync(x => x.Email == email, cancellationToken);
    Assert.False(string.IsNullOrWhiteSpace(createdUser.PasswordHash));
    Assert.True(createdUser.IsActive);
  }

  [Fact]
  public async Task PutUsers_WhenAdministratorUpdatesUser_ShouldReturnOk()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var userRoleId = await RolesTestHelper.GetRoleIdByNameAsync(factory, "User", cancellationToken);
    var email = $"updated-{Guid.NewGuid():N}@example.com";

    var createResponse = await admin.Client.PostAsJsonAsync("/api/v1/users", new
    {
      FirstName = "Original",
      LastName = "User",
      Email = email,
      Password = TestAuthHelper.DefaultUserPassword,
      RoleIds = new[] { userRoleId }
    }, cancellationToken);

    createResponse.EnsureSuccessStatusCode();
    var createBody = await createResponse.Content.ReadAsStringAsync(cancellationToken);
    var userId = RolesTestHelper.ReadGuidFromResultBody(createBody, "id");

    var updateResponse = await admin.Client.PutAsJsonAsync($"/api/v1/users/{userId}", new
    {
      Id = userId,
      FirstName = "Updated",
      LastName = "User",
      Email = email,
      RoleIds = new[] { userRoleId }
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
  }
}
