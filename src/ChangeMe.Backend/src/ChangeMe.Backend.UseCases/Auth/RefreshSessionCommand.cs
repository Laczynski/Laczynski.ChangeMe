using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using ChangeMe.Backend.UseCases.Auth.Utils;

namespace ChangeMe.Backend.UseCases.Auth;

public sealed record RefreshSessionCommand(string RefreshToken) : ICommand<AuthResponseDto>;

public class RefreshSessionHandler(
  ApplicationDbContext context,
  IJwtTokenGenerator jwtTokenGenerator,
  ISessionLifetimeService sessionLifetime) : ICommandHandler<RefreshSessionCommand, AuthResponseDto>
{
  public async ValueTask<Result<AuthResponseDto>> Handle(RefreshSessionCommand command, CancellationToken cancellationToken)
  {
    if (string.IsNullOrWhiteSpace(command.RefreshToken))
      return Result<AuthResponseDto>.Unauthorized();

    var refreshTokenHash = RefreshTokenGenerator.HashToken(command.RefreshToken);
    var utcNow = DateTime.UtcNow;

    var session = await context.UserSessions
      .FirstOrDefaultAsync(x => x.RefreshTokenHash == refreshTokenHash, cancellationToken);

    if (session is null || !sessionLifetime.IsActive(session, utcNow))
      return Result<AuthResponseDto>.Unauthorized();

    var user = await context.Users
      .FirstOrDefaultAsync(x => x.Id == session.UserId, cancellationToken);
    if (user is null || !user.IsActive)
      return Result<AuthResponseDto>.Unauthorized();

    var newRefreshToken = RefreshTokenGenerator.CreateToken();
    var newRefreshTokenHash = RefreshTokenGenerator.HashToken(newRefreshToken);
    var refreshTokenExpiresAtUtc = sessionLifetime.GetRefreshTokenExpiresAtUtc(session.SignedInAt);
    session.RotateRefreshToken(newRefreshTokenHash, refreshTokenExpiresAtUtc);

    await context.SaveChangesAsync(cancellationToken);

    return await AuthSessionUtils.CreateAuthResponseAsync(
      context,
      jwtTokenGenerator,
      user,
      session,
      newRefreshToken,
      cancellationToken);
  }
}
