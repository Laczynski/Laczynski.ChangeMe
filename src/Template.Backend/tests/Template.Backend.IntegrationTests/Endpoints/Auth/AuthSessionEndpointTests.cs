using System.Net;
using System.Net.Http.Json;
using DataGrid.Abstractions;
using DataGrid.Abstractions.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;
using Template.Backend.IntegrationTests.Fixtures;
using Template.Backend.IntegrationTests.Support;
using Template.Backend.UseCases.Auth.Dtos;

namespace Template.Backend.IntegrationTests.Endpoints.Auth;

[Collection(IntegrationTestCollection.Name)]
public sealed class AuthSessionEndpointTests(BackendWebApplicationFactory factory)
{
  [Fact]
  public async Task PostRefresh_WhenRefreshTokenIsValid_ShouldReturnNewTokens()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var session = await TestAuthHelper.CreateLoginSessionAsync(factory, cancellationToken);

    using var refreshClient = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var response = await refreshClient.PostAsJsonAsync("/api/v1/auth/refresh", new
    {
      RefreshToken = session.RefreshToken
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var refreshed = await IntegrationApiJson.ReadValueAsync<AuthResponseDto>(response.Content, cancellationToken);
    Assert.NotNull(refreshed);
    Assert.Equal(session.UserId, refreshed.UserId);
    Assert.Equal(session.SessionId, refreshed.SessionId);
    Assert.False(string.IsNullOrWhiteSpace(refreshed.Token));
    Assert.False(string.IsNullOrWhiteSpace(refreshed.RefreshToken));

    var reuseOldRefresh = await refreshClient.PostAsJsonAsync("/api/v1/auth/refresh", new
    {
      RefreshToken = session.RefreshToken
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.Unauthorized, reuseOldRefresh.StatusCode);
  }

  [Fact]
  public async Task PostRefresh_WhenRefreshTokenIsInvalid_ShouldReturnUnauthorized()
  {
    var cancellationToken = TestContext.Current.CancellationToken;

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var response = await client.PostAsJsonAsync("/api/v1/auth/refresh", new
    {
      RefreshToken = "invalid-refresh-token"
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
  }

  [Fact]
  public async Task PostRefresh_WhenRefreshTokenWasRotated_ShouldReturnUnauthorized()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var session = await TestAuthHelper.CreateLoginSessionAsync(factory, cancellationToken);

    using var refreshClient = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var firstRefresh = await refreshClient.PostAsJsonAsync("/api/v1/auth/refresh", new
    {
      RefreshToken = session.RefreshToken
    }, cancellationToken);
    firstRefresh.EnsureSuccessStatusCode();

    var secondRefresh = await refreshClient.PostAsJsonAsync("/api/v1/auth/refresh", new
    {
      RefreshToken = session.RefreshToken
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.Unauthorized, secondRefresh.StatusCode);
  }

  [Fact]
  public async Task GetMyAccount_WhenAuthenticated_ShouldReturnProfile()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var session = await TestAuthHelper.CreateLoginSessionAsync(factory, cancellationToken);

    var response = await session.Client.GetAsync("/api/v1/auth/account", cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var account = await IntegrationApiJson.ReadValueAsync<MyAccountDto>(response.Content, cancellationToken);
    Assert.NotNull(account);
    Assert.Equal(session.UserId, account.Id);
    Assert.Equal(session.Email, account.Email);
    Assert.Equal("Test", account.FirstName);
    Assert.Equal("User", account.LastName);
    Assert.NotEmpty(account.EffectivePermissions);
  }

  [Fact]
  public async Task PutMyAccount_WhenValid_ShouldUpdateProfile()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var session = await TestAuthHelper.CreateLoginSessionAsync(factory, cancellationToken);

    var accountResponse = await session.Client.GetAsync("/api/v1/auth/account", cancellationToken);
    accountResponse.EnsureSuccessStatusCode();
    var account = await IntegrationApiJson.ReadValueAsync<MyAccountDto>(accountResponse.Content, cancellationToken);
    Assert.NotNull(account);

    var response = await session.Client.PutAsJsonAsync("/api/v1/auth/account", new
    {
      Version = account.Version,
      FirstName = "Updated",
      LastName = "Profile"
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var updated = await IntegrationApiJson.ReadValueAsync<MyAccountDto>(response.Content, cancellationToken);
    Assert.NotNull(updated);
    Assert.Equal("Updated", updated.FirstName);
    Assert.Equal("Profile", updated.LastName);
    Assert.True(updated.Version > account.Version);
  }

  [Fact]
  public async Task PostLogout_WhenAuthenticated_ShouldReturnOk()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var session = await TestAuthHelper.CreateLoginSessionAsync(factory, cancellationToken);

    var response = await session.Client.PostAsync("/api/v1/auth/logout", null, cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  public async Task GetMySessions_WhenUserHasMultipleSessions_ShouldListThem()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var firstSession = await TestAuthHelper.CreateLoginSessionAsync(factory, cancellationToken);
    var secondSession = await TestAuthHelper.LoginExistingUserAsync(
      factory,
      firstSession.Email,
      TestAuthHelper.DefaultUserPassword,
      cancellationToken);

    var grid = GridQueryJson.Serialize(new GridQuery { Take = 10 });
    var response = await secondSession.Client.GetAsync(
      $"/api/v1/auth/sessions?grid={Uri.EscapeDataString(grid)}",
      cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var sessions = await IntegrationApiJson.ReadValueAsync<GridResult<UserSessionDto>>(response.Content, cancellationToken);
    Assert.NotNull(sessions);
    Assert.True(sessions.TotalCount >= 2);
    Assert.Contains(sessions.Items, x => x.Id == firstSession.SessionId);
    Assert.Contains(sessions.Items, x => x.Id == secondSession.SessionId && x.IsCurrent);
  }

  [Fact]
  public async Task DeleteMySession_WhenRevokingOtherSession_ShouldReturnOk()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var firstSession = await TestAuthHelper.CreateLoginSessionAsync(factory, cancellationToken);
    var secondSession = await TestAuthHelper.LoginExistingUserAsync(
      factory,
      firstSession.Email,
      TestAuthHelper.DefaultUserPassword,
      cancellationToken);

    var response = await secondSession.Client.DeleteAsync(
      $"/api/v1/auth/sessions/{firstSession.SessionId}",
      cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    using var refreshClient = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var refreshResponse = await refreshClient.PostAsJsonAsync("/api/v1/auth/refresh", new
    {
      RefreshToken = firstSession.RefreshToken
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);
  }

  [Fact]
  public async Task PostLogoutAll_ShouldRevokeOtherSessions()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var firstSession = await TestAuthHelper.CreateLoginSessionAsync(factory, cancellationToken);
    var secondSession = await TestAuthHelper.LoginExistingUserAsync(
      factory,
      firstSession.Email,
      TestAuthHelper.DefaultUserPassword,
      cancellationToken);

    var response = await secondSession.Client.PostAsync("/api/v1/auth/logout-all", null, cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    using var refreshClient = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var refreshResponse = await refreshClient.PostAsJsonAsync("/api/v1/auth/refresh", new
    {
      RefreshToken = firstSession.RefreshToken
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);
  }
}
