using DataGrid.Abstractions;
using Template.Backend.UseCases.Roles.Dtos;
using Template.Backend.UseCases.Roles.Users;

namespace Template.Backend.Web.Endpoints.Roles.Users;

public class GetRoleAssignedUsers(IMediator mediator)
  : BaseEndpoint<GetRoleAssignedUsersQuery, GridResult<RoleAssignedUserDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.RolesView);
    Get("/roles/{RoleId}/users");
    Summary(s => s.Summary = "Get users assigned to role");
  }
}
