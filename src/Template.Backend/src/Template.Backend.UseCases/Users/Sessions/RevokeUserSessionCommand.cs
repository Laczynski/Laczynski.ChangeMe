
namespace Template.Backend.UseCases.Users.Sessions;

public sealed record RevokeUserSessionCommand(Guid Id, Guid SessionId) : ICommand<bool>;

public class RevokeUserSessionHandler(
  ApplicationDbContext context) : ICommandHandler<RevokeUserSessionCommand, bool>
{
  public async ValueTask<Result<bool>> Handle(RevokeUserSessionCommand command, CancellationToken cancellationToken)
  {
    var session = await context.UserSessions
      .FirstOrDefaultAsync(
        x => x.Id == command.SessionId && x.UserId == command.Id,
        cancellationToken);

    if (session is null || session.IsRevoked)
      return Result<bool>.NotFound();

    session.Revoke(DateTime.UtcNow);
    await context.SaveChangesAsync(cancellationToken);
    return Result.Success(true);
  }
}
