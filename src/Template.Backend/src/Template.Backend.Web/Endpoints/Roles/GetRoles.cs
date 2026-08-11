using DataGrid.Abstractions;
using Template.Backend.UseCases.Roles;
using Template.Backend.UseCases.Roles.Dtos;

namespace Template.Backend.Web.Endpoints.Roles;

public class GetRoles(IMediator mediator) : BaseEndpoint<GetRolesQuery, GridResult<RoleListItemDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.RolesView);
    Get("/roles");
    Summary(s => s.Summary = "Get roles");
  }
}
