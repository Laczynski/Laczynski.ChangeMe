using Template.Backend.Domain.Aggregates.Sessions;

namespace Template.Backend.Infrastructure.Auth;

public interface ISessionLifetimeService
{
  int SessionLifetimeDays { get; }

  bool IsActive(UserSession session, DateTime utcNow);

  DateTime GetRefreshTokenExpiresAtUtc(DateTime signedInAtUtc);
}
