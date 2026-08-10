using Template.Backend.UseCases.Roles;
using Template.Backend.UseCases.Roles.Dtos;

namespace Template.Backend.Web.Endpoints.Roles;

public class GetRoleById(IMediator mediator) : BaseEndpoint<GetRoleByIdQuery, RoleDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.RolesView);
    Get("/roles/{Id}");
    Summary(s => s.Summary = "Get role by id");
  }
}
