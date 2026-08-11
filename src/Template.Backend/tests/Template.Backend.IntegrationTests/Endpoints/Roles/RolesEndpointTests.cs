using System.Net;
using System.Net.Http.Json;
using DataGrid.Abstractions;
using DataGrid.Abstractions.Serialization;
using Template.Backend.Domain.Aggregates.Roles;
using Template.Backend.Domain.Authorization;
using Template.Backend.IntegrationTests.Fixtures;
using Template.Backend.IntegrationTests.Support;
using Template.Backend.UseCases.Roles.Utils;

namespace Template.Backend.IntegrationTests.Endpoints.Roles;

[Collection(IntegrationTestCollection.Name)]
public sealed class RolesEndpointTests(BackendWebApplicationFactory factory)
{
  [Fact]
  public async Task GetRoles_WhenUserLacksPermission_ShouldReturnForbidden()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);

    var response = await user.Client.GetAsync("/api/v1/roles", cancellationToken);

    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
  }

  [Fact]
  public async Task GetRoles_WhenAdministrator_ShouldReturnSeededRoles()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);

    var response = await admin.Client.GetAsync("/api/v1/roles", cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(RoleConstraints.AdministratorRoleName, body, StringComparison.OrdinalIgnoreCase);
    Assert.Contains(RoleConstraints.UserRoleName, body, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public async Task GetRoles_WhenSearchMatchesDescription_ShouldReturnMatchingRole()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var marker = $"search-marker-{Guid.NewGuid():N}";
    await RolesTestHelper.CreateCustomRoleAsync(
      admin.Client,
      cancellationToken,
      description: marker);

    var grid = GridQueryJson.Serialize(new GridQuery { Take = 10, Search = marker });

    var response = await admin.Client.GetAsync(
      $"/api/v1/roles?grid={Uri.EscapeDataString(grid)}",
      cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(marker, body, StringComparison.Ordinal);
  }

  [Fact]
  public async Task GetRoleById_WhenAdministrator_ShouldReturnOk()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var roleId = await RolesTestHelper.CreateCustomRoleAsync(admin.Client, cancellationToken);

    var response = await admin.Client.GetAsync($"/api/v1/roles/{roleId}", cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  public async Task GetRoleById_WhenUserLacksPermission_ShouldReturnForbidden()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var roleId = await RolesTestHelper.CreateCustomRoleAsync(admin.Client, cancellationToken);

    var response = await user.Client.GetAsync($"/api/v1/roles/{roleId}", cancellationToken);

    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
  }

  [Fact]
  public async Task GetRoleById_WhenRoleDoesNotExist_ShouldReturnNotFound()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);

    var response = await admin.Client.GetAsync($"/api/v1/roles/{Guid.NewGuid()}", cancellationToken);

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task GetPermissionCatalog_WhenAdministrator_ShouldReturnCatalog()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);

    var response = await admin.Client.GetAsync("/api/v1/roles/permission-catalog", cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(PermissionCodes.UsersView, body, StringComparison.Ordinal);
    Assert.Contains(PermissionCodes.RolesManage, body, StringComparison.Ordinal);
  }

  [Fact]
  public async Task GetPermissionCatalog_WhenUserLacksPermission_ShouldReturnForbidden()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);

    var response = await user.Client.GetAsync("/api/v1/roles/permission-catalog", cancellationToken);

    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
  }

  [Fact]
  public async Task PostRoles_WhenAdministratorCreatesRole_ShouldReturnCreated()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var roleName = $"Custom-{Guid.NewGuid():N}";

    var response = await admin.Client.PostAsJsonAsync("/api/v1/roles", new
    {
      Name = roleName,
      Description = "Test role",
      PermissionCodes = new[] { PermissionCodes.UsersView }
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);

    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(roleName, body, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public async Task PostRoles_WhenUserLacksPermission_ShouldReturnForbidden()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);

    var response = await user.Client.PostAsJsonAsync("/api/v1/roles", new
    {
      Name = $"Denied-{Guid.NewGuid():N}",
      PermissionCodes = new[] { PermissionCodes.UsersView }
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
  }

  [Fact]
  public async Task PostRoles_WhenDuplicateName_ShouldReturnConflict()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var roleName = $"Duplicate-{Guid.NewGuid():N}";
    await RolesTestHelper.CreateCustomRoleAsync(admin.Client, cancellationToken, name: roleName);

    var response = await admin.Client.PostAsJsonAsync("/api/v1/roles", new
    {
      Name = roleName.ToUpperInvariant(),
      PermissionCodes = new[] { PermissionCodes.UsersView }
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(RolesUtils.DuplicateNameMessage, body, StringComparison.Ordinal);
  }

  [Fact]
  public async Task PostRoles_WhenNoPermissions_ShouldReturnBadRequest()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);

    var response = await admin.Client.PostAsJsonAsync("/api/v1/roles", new
    {
      Name = $"NoPerms-{Guid.NewGuid():N}",
      PermissionCodes = Array.Empty<string>()
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task PutRole_WhenAdministratorUpdatesCustomRole_ShouldReturnOk()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var roleId = await RolesTestHelper.CreateCustomRoleAsync(admin.Client, cancellationToken);
    var updatedName = $"Updated-{Guid.NewGuid():N}";

    var response = await admin.Client.PutAsJsonAsync($"/api/v1/roles/{roleId}", new
    {
      Id = roleId,
      Name = updatedName,
      Description = "Updated description",
      PermissionCodes = new[] { PermissionCodes.UsersView, PermissionCodes.RolesView }
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(updatedName, body, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public async Task PutRole_WhenSystemRole_ShouldReturnBadRequest()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var administratorRoleId = await RolesTestHelper.GetRoleIdByNameAsync(
      factory,
      RoleConstraints.AdministratorRoleName,
      cancellationToken);

    var response = await admin.Client.PutAsJsonAsync($"/api/v1/roles/{administratorRoleId}", new
    {
      Id = administratorRoleId,
      Name = "Renamed Administrator",
      PermissionCodes = PermissionCodes.All
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(RolesUtils.SystemRoleCannotBeModifiedMessage, body, StringComparison.Ordinal);
  }

  [Fact]
  public async Task DeleteRole_WhenCustomRoleUnassigned_ShouldReturnOk()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var roleId = await RolesTestHelper.CreateCustomRoleAsync(admin.Client, cancellationToken);

    var response = await admin.Client.DeleteAsync($"/api/v1/roles/{roleId}", cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  public async Task DeleteRole_WhenRoleHasUsers_ShouldReturnBadRequest()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var roleId = await RolesTestHelper.CreateCustomRoleAsync(admin.Client, cancellationToken);
    var (userId, email) = await RolesTestHelper.CreateManagedUserAsync(admin.Client, factory, cancellationToken);
    var userRoleId = await RolesTestHelper.GetRoleIdByNameAsync(factory, RoleConstraints.UserRoleName, cancellationToken);

    await RolesTestHelper.AssignUserRolesAsync(
      admin.Client,
      userId,
      email,
      [userRoleId, roleId],
      cancellationToken);

    var response = await admin.Client.DeleteAsync($"/api/v1/roles/{roleId}", cancellationToken);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(RolesUtils.RoleAssignedToUsersMessage, body, StringComparison.Ordinal);
  }

  [Fact]
  public async Task DeleteRole_WhenSystemRole_ShouldReturnBadRequest()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var userRoleId = await RolesTestHelper.GetRoleIdByNameAsync(
      factory,
      RoleConstraints.UserRoleName,
      cancellationToken);

    var response = await admin.Client.DeleteAsync($"/api/v1/roles/{userRoleId}", cancellationToken);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task GetRoleAssignedUsers_WhenUsersAssigned_ShouldReturnUsers()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var roleId = await RolesTestHelper.CreateCustomRoleAsync(admin.Client, cancellationToken);
    var userRoleId = await RolesTestHelper.GetRoleIdByNameAsync(factory, RoleConstraints.UserRoleName, cancellationToken);
    var (userId, email) = await RolesTestHelper.CreateManagedUserAsync(admin.Client, factory, cancellationToken);

    await RolesTestHelper.AssignUserRolesAsync(
      admin.Client,
      userId,
      email,
      new[] { userRoleId, roleId },
      cancellationToken);

    var response = await admin.Client.GetAsync($"/api/v1/roles/{roleId}/users", cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    using var document = System.Text.Json.JsonDocument.Parse(body);
    var assignedUser = document.RootElement
      .GetProperty("value")
      .GetProperty("items")
      .EnumerateArray()
      .First(item => item.GetProperty("id").GetGuid() == userId);

    Assert.Equal("Role", assignedUser.GetProperty("firstName").GetString());
    Assert.Equal("Assignee", assignedUser.GetProperty("lastName").GetString());
  }

  [Fact]
  public async Task DeleteRemoveUserFromRole_WhenUserHasOtherRole_ShouldReturnOk()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var customRoleId = await RolesTestHelper.CreateCustomRoleAsync(admin.Client, cancellationToken);
    var userRoleId = await RolesTestHelper.GetRoleIdByNameAsync(factory, RoleConstraints.UserRoleName, cancellationToken);
    var (userId, email) = await RolesTestHelper.CreateManagedUserAsync(admin.Client, factory, cancellationToken);

    await RolesTestHelper.AssignUserRolesAsync(
      admin.Client,
      userId,
      email,
      new[] { userRoleId, customRoleId },
      cancellationToken);

    var response = await admin.Client.DeleteAsync(
      $"/api/v1/roles/{customRoleId}/users/{userId}",
      cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  public async Task DeleteRemoveUserFromRole_WhenUserWouldHaveZeroRoles_ShouldReturnBadRequest()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var userRoleId = await RolesTestHelper.GetRoleIdByNameAsync(factory, RoleConstraints.UserRoleName, cancellationToken);
    var (userId, _) = await RolesTestHelper.CreateManagedUserAsync(admin.Client, factory, cancellationToken);

    var response = await admin.Client.DeleteAsync(
      $"/api/v1/roles/{userRoleId}/users/{userId}",
      cancellationToken);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(RolesUtils.UserMustHaveRoleMessage, body, StringComparison.Ordinal);
  }

}
