using ChangeMe.Backend.UseCases.Roles.Utils;

namespace ChangeMe.Backend.UseCases.Roles.Users;

public sealed record RemoveUserFromRoleCommand(Guid RoleId, Guid UserId) : ICommand<bool>;

public class RemoveUserFromRoleHandler(
  ApplicationDbContext context) : ICommandHandler<RemoveUserFromRoleCommand, bool>
{
  public async ValueTask<Result<bool>> Handle(RemoveUserFromRoleCommand command, CancellationToken cancellationToken)
  {
    var roleExists = await context.Roles.AsNoTracking().AnyAsync(x => x.Id == command.RoleId, cancellationToken);
    if (!roleExists)
      return Result<bool>.NotFound();

    var user = await context.Users
      .Include(x => x.Roles)
      .FirstOrDefaultAsync(x => x.Id == command.UserId, cancellationToken);

    if (user is null || !user.HasRole(command.RoleId))
      return Result<bool>.NotFound();

    var removeResult = user.RemoveRole(command.RoleId);
    if (!removeResult.IsSuccess)
      return removeResult.Status == ResultStatus.Error
        ? Result<bool>.Error(RolesUtils.UserMustHaveRoleMessage)
        : removeResult.Map();

    await context.SaveChangesAsync(cancellationToken);
    return Result.Success(true);
  }
}
