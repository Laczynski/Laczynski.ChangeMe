using ChangeMe.Backend.UseCases.Roles.Dtos;
using ChangeMe.Backend.UseCases.Roles.Users;
using DataGrid.Abstractions;

namespace ChangeMe.Backend.Web.Endpoints.Roles.Users;

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
