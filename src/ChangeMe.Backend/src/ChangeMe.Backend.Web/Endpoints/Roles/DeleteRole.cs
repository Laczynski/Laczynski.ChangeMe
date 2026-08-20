using ChangeMe.Backend.UseCases.Roles;

namespace ChangeMe.Backend.Web.Endpoints.Roles;

public class DeleteRole(IMediator mediator) : BaseEndpoint<DeleteRoleCommand, bool>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.RolesManage);
    Delete("/roles/{Id}");
    Summary(s => s.Summary = "Delete role");
  }
}
