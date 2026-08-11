namespace Template.Backend.UseCases.Auth.Sessions;

public sealed record RevokeMySessionCommand(Guid SessionId) : ICommand<bool>;

public class RevokeMySessionHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : ICommandHandler<RevokeMySessionCommand, bool>
{
  public async ValueTask<Result<bool>> Handle(RevokeMySessionCommand command, CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid userId)
      return Result<bool>.Unauthorized();

    if (userAccessor.SessionId == command.SessionId)
      return Result<bool>.Error("Cannot revoke the current session.");

    var session = await context.UserSessions
      .FirstOrDefaultAsync(x => x.Id == command.SessionId && x.UserId == userId, cancellationToken);

    if (session is null || session.IsRevoked)
      return Result<bool>.NotFound();

    session.Revoke(DateTime.UtcNow);
    await context.SaveChangesAsync(cancellationToken);
    return Result.Success(true);
  }
}
