
using Template.Backend.UseCases.Users.Utils;

namespace Template.Backend.UseCases.Users.Sessions;

public sealed record RevokeAllUserSessionsCommand(Guid Id) : ICommand<bool>;

public class RevokeAllUserSessionsHandler(
  ApplicationDbContext context) : ICommandHandler<RevokeAllUserSessionsCommand, bool>
{
  public async ValueTask<Result<bool>> Handle(RevokeAllUserSessionsCommand command, CancellationToken cancellationToken)
  {
    var userExists = await context.Users.AnyAsync(x => x.Id == command.Id, cancellationToken);
    if (!userExists)
      return Result<bool>.NotFound();

    await UsersUtils.RevokeAllActiveSessionsAsync(context, command.Id, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);
    return Result.Success(true);
  }
}
