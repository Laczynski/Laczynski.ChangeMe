using ChangeMe.Backend.Domain.Aggregates.Sessions;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.Infrastructure.Auth;

public sealed class SessionLifetimeService(IOptions<AuthOptions> options) : ISessionLifetimeService
{
  private readonly JwtOptions jwtOptions = options.Value.Jwt;

  public int SessionLifetimeDays =>
    jwtOptions.SessionLifetimeDays > 0
      ? jwtOptions.SessionLifetimeDays
      : 14;

  public bool IsActive(UserSession session, DateTime utcNow) =>
    session.IsActive(utcNow, SessionLifetimeDays);

  public DateTime GetRefreshTokenExpiresAtUtc(DateTime signedInAtUtc) =>
    signedInAtUtc.AddDays(SessionLifetimeDays);
}
