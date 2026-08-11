using Template.Backend.Domain.Aggregates.Roles;
using Template.Backend.UseCases.Roles;
using Template.Backend.UseCases.Roles.Dtos;

namespace Template.Backend.Web.Endpoints.Roles;

public class CreateRole(IMediator mediator) : BaseEndpoint<CreateRoleCommand, RoleDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.RolesManage);
    Post("/roles");
    Summary(s => s.Summary = "Create role");
  }
}

public sealed class CreateRoleCommandValidator : Validator<CreateRoleCommand>
{
  public CreateRoleCommandValidator()
  {
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
