using ChangeMe.Backend.UseCases.Roles.Users;

namespace ChangeMe.Backend.Web.Endpoints.Roles.Users;

public class RemoveUserFromRole(IMediator mediator) : BaseEndpoint<RemoveUserFromRoleCommand, bool>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.RolesManage);
    Delete("/roles/{RoleId}/users/{UserId}");
    Summary(s => s.Summary = "Remove user from role");
  }
}
