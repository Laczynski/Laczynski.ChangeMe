using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ChangeMe.Backend.Infrastructure.Persistence;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeMe.Backend.IntegrationTests.Support;

internal static class TestAuthHelper
{
  public const string SeededAdminEmail = "integration-admin@example.com";
  public const string SeededAdminPassword = "IntegrationAdmin123!";
  public const string DefaultUserPassword = "StrongPass123!";

  public static async Task<HttpClient> CreateAuthenticatedClientAsync(
    BackendWebApplicationFactory factory,
    CancellationToken cancellationToken = default)
  {
    var user = await CreateAuthenticatedUserAsync(factory, cancellationToken);
    return user.Client;
  }

  public static async Task<AuthenticatedTestUser> CreateAuthenticatedUserAsync(
    BackendWebApplicationFactory factory,
    CancellationToken cancellationToken = default)
  {
    var session = await CreateLoginSessionAsync(factory, cancellationToken);
    return new AuthenticatedTestUser(session.Client, session.UserId, session.Email);
  }

  public static async Task<LoginTestSession> CreateLoginSessionAsync(
    BackendWebApplicationFactory factory,
    CancellationToken cancellationToken = default)
  {
    var admin = await CreateAdministratorUserAsync(factory, cancellationToken);
    var email = $"user-{Guid.NewGuid():N}@example.com";
    var userRoleId = await RolesTestHelper.GetRoleIdByNameAsync(factory, "User", cancellationToken);

    var createResponse = await admin.Client.PostAsJsonAsync("/api/v1/users", new
    {
      FirstName = "Test",
      LastName = "User",
      Email = email,
      Password = DefaultUserPassword,
      RoleIds = new[] { userRoleId }
    }, cancellationToken);

    createResponse.EnsureSuccessStatusCode();

    return await LoginExistingUserAsync(factory, email, DefaultUserPassword, cancellationToken);
  }

  public static async Task<AuthenticatedTestUser> CreateAdministratorUserAsync(
    BackendWebApplicationFactory factory,
    CancellationToken cancellationToken = default)
  {
    var session = await LoginExistingUserAsync(
      factory,
      SeededAdminEmail,
      SeededAdminPassword,
      cancellationToken);

    return new AuthenticatedTestUser(session.Client, session.UserId, session.Email);
  }

  public static async Task<LoginTestSession> LoginExistingUserAsync(
    BackendWebApplicationFactory factory,
    string email,
    string password,
    CancellationToken cancellationToken)
  {
    using var loginClient = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var loginResponse = await loginClient.PostAsJsonAsync("/api/v1/auth/login", new
    {
      Email = email,
      Password = password
    }, cancellationToken);

    loginResponse.EnsureSuccessStatusCode();

    var responseBody = await loginResponse.Content.ReadAsStringAsync(cancellationToken);
    var authSession = ParseLoginAuthSession(responseBody);

    var authenticatedClient = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    authenticatedClient.DefaultRequestHeaders.Authorization =
      new AuthenticationHeaderValue("Bearer", authSession.Token);

    await using var scope = factory.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userId = await dbContext.Users
      .AsNoTracking()
      .Where(u => u.Email == email)
      .Select(u => u.Id)
      .SingleAsync(cancellationToken);

    return new LoginTestSession(
      authenticatedClient,
      userId,
      email,
      authSession.SessionId,
      authSession.Token,
      authSession.RefreshToken);
  }

  private static AuthResponseDto ParseLoginAuthSession(string responseBody)
  {
    using var document = JsonDocument.Parse(responseBody);

    if (!document.RootElement.TryGetProperty("value", out var valueElement))
      throw new InvalidOperationException("Login response did not contain a value payload.");

    if (valueElement.TryGetProperty("authSession", out var authSessionElement))
    {
      var authSession = authSessionElement.Deserialize<AuthResponseDto>(IntegrationApiJson.SerializerOptions);
      return authSession ?? throw new InvalidOperationException("Auth session payload is null.");
    }

    var directAuthSession = valueElement.Deserialize<AuthResponseDto>(IntegrationApiJson.SerializerOptions);
    return directAuthSession ?? throw new InvalidOperationException("Auth session payload is null.");
  }
}

internal sealed record AuthenticatedTestUser(HttpClient Client, Guid UserId, string Email);

internal sealed record LoginTestSession(
  HttpClient Client,
  Guid UserId,
  string Email,
  Guid SessionId,
  string AccessToken,
  string RefreshToken);
