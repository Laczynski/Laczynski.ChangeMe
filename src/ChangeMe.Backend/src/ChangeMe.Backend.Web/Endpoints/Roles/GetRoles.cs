using ChangeMe.Backend.UseCases.Roles;
using ChangeMe.Backend.UseCases.Roles.Dtos;
using DataGrid.Abstractions;

namespace ChangeMe.Backend.Web.Endpoints.Roles;

public class GetRoles(IMediator mediator) : BaseEndpoint<GetRolesQuery, GridResult<RoleListItemDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.RolesView);
    Get("/roles");
    Summary(s => s.Summary = "Get roles");
  }
}
