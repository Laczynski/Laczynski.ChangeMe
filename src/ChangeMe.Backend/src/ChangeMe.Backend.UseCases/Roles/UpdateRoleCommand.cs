using ChangeMe.Backend.UseCases.Roles.Dtos;
using ChangeMe.Backend.UseCases.Roles.Utils;

namespace ChangeMe.Backend.UseCases.Roles;

public sealed record UpdateRoleCommand(
  Guid Id,
  long Version,
  string Name,
  string? Description,
  IReadOnlyList<string> PermissionCodes) : ICommand<RoleDetailsDto>;

public class UpdateRoleHandler(
  IMediator mediator,
  ApplicationDbContext context) : ICommandHandler<UpdateRoleCommand, RoleDetailsDto>
{
  public async ValueTask<Result<RoleDetailsDto>> Handle(UpdateRoleCommand command, CancellationToken cancellationToken)
  {
    var role = await context.Roles
      .Include(x => x.Permissions)
      .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

    if (role is null)
      return Result<RoleDetailsDto>.NotFound();

    var versionCheck = ConcurrencyGuard.CheckExpectedVersion(role, command.Version);
    if (!versionCheck.IsSuccess)
      return versionCheck.Map();

    if (role.IsSystem)
      return Result<RoleDetailsDto>.Error(RolesUtils.SystemRoleCannotBeModifiedMessage);

    if (await RolesUtils.IsNameTakenAsync(context, command.Name, role.Id, cancellationToken))
      return Result<RoleDetailsDto>.Conflict(RolesUtils.DuplicateNameMessage);

    var permissionValidation = RolesUtils.ValidatePermissionCodes(command.PermissionCodes);
    if (!permissionValidation.IsSuccess)
      return permissionValidation.Map();

    var updateResult = role.UpdateProfile(command.Name, command.Description);
    if (!updateResult.IsSuccess)
      return updateResult.Map();

    role.SetPermissions(command.PermissionCodes);
    await context.SaveChangesAsync(cancellationToken);

    var updatedRoleResult = await mediator.Send(new GetRoleByIdQuery { Id = role.Id }, cancellationToken);
    if (!updatedRoleResult.IsSuccess)
      return updatedRoleResult.Map();

    return updatedRoleResult;
  }
}
