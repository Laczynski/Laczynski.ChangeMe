using Template.Backend.UseCases.Users;
using Template.Backend.UseCases.Users.Dtos;

namespace Template.Backend.Web.Endpoints.Users;

public class PreviewEffectivePermissions(IMediator mediator)
  : BaseEndpoint<PreviewEffectivePermissionsQuery, IReadOnlyList<EffectivePermissionDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.UsersManage);
    Get("/users/effective-permissions/preview");
    Summary(s => s.Summary = "Preview effective permissions for selected roles");
  }
}
