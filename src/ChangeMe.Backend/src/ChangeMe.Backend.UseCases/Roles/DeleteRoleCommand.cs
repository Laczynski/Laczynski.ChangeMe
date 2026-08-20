
using ChangeMe.Backend.UseCases.Roles.Utils;

namespace ChangeMe.Backend.UseCases.Roles;

public sealed record DeleteRoleCommand(Guid Id) : ICommand<bool>;

public class DeleteRoleHandler(
  ApplicationDbContext context) : ICommandHandler<DeleteRoleCommand, bool>
{
  public async ValueTask<Result<bool>> Handle(DeleteRoleCommand command, CancellationToken cancellationToken)
  {
    var role = await context.Roles.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
    if (role is null)
      return Result<bool>.NotFound();

    if (role.IsSystem)
      return Result<bool>.Error(RolesUtils.SystemRoleCannotBeModifiedMessage);

    var hasAssignments = await context.Users.AnyAsync(
      u => u.Roles.Any(ur => ur.RoleId == role.Id),
      cancellationToken);
    if (hasAssignments)
      return Result<bool>.Error(RolesUtils.RoleAssignedToUsersMessage);

    context.Roles.Remove(role);
    await context.SaveChangesAsync(cancellationToken);
    return Result.Success(true);
  }
}
