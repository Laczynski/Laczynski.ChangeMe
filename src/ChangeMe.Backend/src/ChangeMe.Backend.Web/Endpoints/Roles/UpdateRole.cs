using ChangeMe.Backend.Domain.Aggregates.Roles;
using ChangeMe.Backend.UseCases.Roles;
using ChangeMe.Backend.UseCases.Roles.Dtos;

namespace ChangeMe.Backend.Web.Endpoints.Roles;

public class UpdateRole(IMediator mediator) : BaseEndpoint<UpdateRoleCommand, RoleDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.RolesManage);
    Put("/roles/{Id}");
    Summary(s => s.Summary = "Update role");
  }
}

public sealed class UpdateRoleCommandValidator : Validator<UpdateRoleCommand>
{
  public UpdateRoleCommandValidator()
  {
    RuleFor(x => x.Id).NotEmpty();
    RuleFor(x => x.Version).GreaterThanOrEqualTo(0);
    RuleFor(x => x.Name)
      .NotEmpty()
      .MinimumLength(RoleConstraints.NAME_MIN_LENGTH)
      .MaximumLength(RoleConstraints.NAME_MAX_LENGTH);
    RuleFor(x => x.Description)
      .MaximumLength(RoleConstraints.DESCRIPTION_MAX_LENGTH)
      .When(x => !string.IsNullOrWhiteSpace(x.Description));
    RuleFor(x => x.PermissionCodes).NotEmpty();
  }
}
