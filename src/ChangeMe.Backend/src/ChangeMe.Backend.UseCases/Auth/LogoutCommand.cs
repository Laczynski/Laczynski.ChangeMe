namespace ChangeMe.Backend.UseCases.Auth;

public sealed record LogoutCommand() : ICommand<bool>;

public class LogoutHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : ICommandHandler<LogoutCommand, bool>
{
  public async ValueTask<Result<bool>> Handle(LogoutCommand command, CancellationToken cancellationToken)
  {
    if (userAccessor.SessionId is not Guid sessionId)
      return Result.Success(true);

    var session = await context.UserSessions
      .FirstOrDefaultAsync(x => x.Id == sessionId, cancellationToken);

    if (session is null || session.IsRevoked)
      return Result.Success(true);

    session.Revoke(DateTime.UtcNow);
    await context.SaveChangesAsync(cancellationToken);
    return Result.Success(true);
  }
}
