using ChangeMe.Backend.UseCases.Users;
using ChangeMe.Backend.UseCases.Users.Dtos;

namespace ChangeMe.Backend.Web.Endpoints.Users;

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
