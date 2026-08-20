using ChangeMe.Backend.Domain.Aggregates.Roles;
using ChangeMe.Backend.UseCases.Roles.Dtos;

using ChangeMe.Backend.UseCases.Roles.Utils;

namespace ChangeMe.Backend.UseCases.Roles;

public sealed record CreateRoleCommand(
  string Name,
  string? Description,
  IReadOnlyList<string> PermissionCodes) : ICommand<RoleDetailsDto>;

public class CreateRoleHandler(
  IMediator mediator,
  ApplicationDbContext context) : ICommandHandler<CreateRoleCommand, RoleDetailsDto>
{
  public async ValueTask<Result<RoleDetailsDto>> Handle(CreateRoleCommand command, CancellationToken cancellationToken)
  {
    if (await RolesUtils.IsNameTakenAsync(context, command.Name, null, cancellationToken))
      return Result<RoleDetailsDto>.Conflict(RolesUtils.DuplicateNameMessage);

    var permissionValidation = RolesUtils.ValidatePermissionCodes(command.PermissionCodes);
    if (!permissionValidation.IsSuccess)
      return permissionValidation.Map();

    var createResult = Role.Create(command.Name, command.Description);
    if (!createResult.IsSuccess)
      return createResult.Map();

    var role = createResult.Value;
    role.SetPermissions(command.PermissionCodes);
    await context.Roles.AddAsync(role, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    var createdRoleResult = await mediator.Send(new GetRoleByIdQuery { Id = role.Id }, cancellationToken);
    if (!createdRoleResult.IsSuccess)
      return createdRoleResult.Map();

    return Result.Created(createdRoleResult.Value, $"/roles/{createdRoleResult.Value.Id}");
  }
}
