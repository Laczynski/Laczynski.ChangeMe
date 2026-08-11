using Microsoft.AspNetCore.Http;
using Template.Backend.Domain.Aggregates.Sessions;
using Template.Backend.Domain.Aggregates.Users;
using Template.Backend.Infrastructure.Auth;
using Template.Backend.UseCases.Auth.Utils;
using SignInMethodConstants = Template.Backend.Domain.Aggregates.Sessions.SignInMethods;

namespace Template.Backend.UseCases.Auth;

public static class AuthSessionFactory
{
  public static async Task<Result<(UserSession Session, string RefreshToken)>> CreateSessionAsync(
    ApplicationDbContext context,
    ISessionLifetimeService sessionLifetime,
    IHttpContextAccessor httpContextAccessor,
    User user,
    CancellationToken cancellationToken,
    string signInMethod = SignInMethodConstants.Password)
  {
    var signedInAt = DateTime.UtcNow;
    var refreshToken = RefreshTokenGenerator.CreateToken();
    var refreshTokenHash = RefreshTokenGenerator.HashToken(refreshToken);
    var refreshTokenExpiresAtUtc = sessionLifetime.GetRefreshTokenExpiresAtUtc(signedInAt);
    var httpContext = httpContextAccessor.HttpContext;
    var deviceLabel = ClientInfoParser.ParseDeviceBrowserLabel(httpContext?.Request.Headers.UserAgent);
    var ipAddress = AuthSessionUtils.GetClientIpAddress(httpContext);

    var sessionResult = UserSession.Create(
      user.Id,
      deviceLabel,
      ipAddress,
      refreshTokenHash,
      refreshTokenExpiresAtUtc,
      signedInAt,
      signInMethod);

    if (!sessionResult.IsSuccess)
      return sessionResult.Map();

    await context.UserSessions.AddAsync(sessionResult.Value, cancellationToken);
    return Result.Success((sessionResult.Value, refreshToken));
  }
}
