namespace ChangeMe.Backend.UseCases.Auth.Sessions;

public sealed record LogoutAllSessionsCommand() : ICommand<bool>;

public class LogoutAllSessionsHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : ICommandHandler<LogoutAllSessionsCommand, bool>
{
  public async ValueTask<Result<bool>> Handle(LogoutAllSessionsCommand command, CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid userId)
      return Result<bool>.Unauthorized();

    await RevokeAllActiveSessionsAsync(context, userId, cancellationToken);
    return Result.Success(true);
  }

  internal static async Task RevokeAllActiveSessionsAsync(
    ApplicationDbContext context,
    Guid userId,
    CancellationToken cancellationToken)
  {
    var utcNow = DateTime.UtcNow;
    var sessions = await context.UserSessions
      .Where(x => x.UserId == userId && x.RevokedAt == null)
      .ToListAsync(cancellationToken);

    foreach (var session in sessions)
      session.Revoke(utcNow);

    await context.SaveChangesAsync(cancellationToken);
  }

  internal static async Task RevokeAllActiveSessionsExceptAsync(
    ApplicationDbContext context,
    Guid userId,
    Guid exceptSessionId,
    CancellationToken cancellationToken)
  {
    var utcNow = DateTime.UtcNow;
    var sessions = await context.UserSessions
      .Where(x => x.UserId == userId && x.RevokedAt == null && x.Id != exceptSessionId)
      .ToListAsync(cancellationToken);

    foreach (var session in sessions)
      session.Revoke(utcNow);

    await context.SaveChangesAsync(cancellationToken);
  }
}
