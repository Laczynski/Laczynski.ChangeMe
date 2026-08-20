using ChangeMe.Backend.UseCases.Roles;
using ChangeMe.Backend.UseCases.Roles.Dtos;

namespace ChangeMe.Backend.Web.Endpoints.Roles;

public class GetRoleById(IMediator mediator) : BaseEndpoint<GetRoleByIdQuery, RoleDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.RolesView);
    Get("/roles/{Id}");
    Summary(s => s.Summary = "Get role by id");
  }
}
