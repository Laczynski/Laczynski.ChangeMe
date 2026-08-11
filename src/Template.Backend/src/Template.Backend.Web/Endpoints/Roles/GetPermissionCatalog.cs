using Template.Backend.UseCases.Roles;
using Template.Backend.UseCases.Roles.Dtos;

namespace Template.Backend.Web.Endpoints.Roles;

public class GetPermissionCatalog(IMediator mediator)
  : BaseEndpointWithoutRequest<GetPermissionCatalogQuery, IReadOnlyList<PermissionCatalogItemDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.RolesManage);
    Get("/roles/permission-catalog");
    Summary(s => s.Summary = "Get permission catalog");
  }
}
