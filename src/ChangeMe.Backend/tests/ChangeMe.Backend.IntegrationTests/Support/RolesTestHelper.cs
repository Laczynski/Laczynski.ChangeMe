using System.Net.Http.Json;
using System.Text.Json;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.Infrastructure.Persistence;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeMe.Backend.IntegrationTests.Support;

internal static class RolesTestHelper
{
  public static async Task<Guid> GetRoleIdByNameAsync(
    BackendWebApplicationFactory factory,
    string roleName,
    CancellationToken cancellationToken)
  {
    await using var scope = factory.Services.GetRequiredService<IServiceScopeFactory>().CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    return await dbContext.Roles
      .AsNoTracking()
      .Where(x => x.Name == roleName)
      .Select(x => x.Id)
      .SingleAsync(cancellationToken);
  }

  public static async Task<Guid> CreateCustomRoleAsync(
    HttpClient client,
    CancellationToken cancellationToken,
    string? name = null,
    string? description = null,
    IReadOnlyList<string>? permissionCodes = null)
  {
    var roleName = name ?? $"Role-{Guid.NewGuid():N}";
    var response = await client.PostAsJsonAsync("/api/v1/roles", new
    {
      Name = roleName,
      Description = description ?? "Integration test role",
      PermissionCodes = permissionCodes ?? new[] { PermissionCodes.UsersView }
    }, cancellationToken);

    response.EnsureSuccessStatusCode();
    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    return ReadGuidFromResultBody(body, "id");
  }

  public static async Task<(Guid UserId, string Email)> CreateManagedUserAsync(
    HttpClient adminClient,
    BackendWebApplicationFactory factory,
    CancellationToken cancellationToken)
  {
    var userRoleId = await GetRoleIdByNameAsync(factory, "User", cancellationToken);
    var email = $"role-user-{Guid.NewGuid():N}@example.com";

    var response = await adminClient.PostAsJsonAsync("/api/v1/users", new
    {
      FirstName = "Role",
      LastName = "Assignee",
      Email = email,
      Password = TestAuthHelper.DefaultUserPassword,
      RoleIds = new[] { userRoleId }
    }, cancellationToken);

    response.EnsureSuccessStatusCode();
    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    return (ReadGuidFromResultBody(body, "id"), email);
  }

  public static async Task AssignUserRolesAsync(
    HttpClient adminClient,
    Guid userId,
    string email,
    IReadOnlyList<Guid> roleIds,
    CancellationToken cancellationToken)
  {
    var response = await adminClient.PutAsJsonAsync($"/api/v1/users/{userId}", new
    {
      Id = userId,
      FirstName = "Role",
      LastName = "Assignee",
      Email = email,
      RoleIds = roleIds
    }, cancellationToken);

    response.EnsureSuccessStatusCode();
  }

  public static Guid ReadGuidFromResultBody(string json, string propertyName)
  {
    using var document = JsonDocument.Parse(json);
    var value = GetResultValueElement(document.RootElement);
    return ReadGuidProperty(value, propertyName);
  }

  private static JsonElement GetResultValueElement(JsonElement root)
  {
    if (root.TryGetProperty("value", out var camelValue))
      return camelValue;

    if (root.TryGetProperty("Value", out var pascalValue))
      return pascalValue;

    return root;
  }

  private static Guid ReadGuidProperty(JsonElement element, string propertyName)
  {
    if (element.TryGetProperty(propertyName, out var camelProperty))
      return camelProperty.GetGuid();

    var pascalName = char.ToUpperInvariant(propertyName[0]) + propertyName[1..];
    return element.GetProperty(pascalName).GetGuid();
  }
}
