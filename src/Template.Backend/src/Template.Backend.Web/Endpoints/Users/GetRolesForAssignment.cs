using Template.Backend.UseCases.Users;
using Template.Backend.UseCases.Users.Dtos;

namespace Template.Backend.Web.Endpoints.Users;

public class GetRolesForAssignment(IMediator mediator)
  : BaseEndpointWithoutRequest<GetRolesForAssignmentQuery, IReadOnlyList<RoleAssignmentOptionDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.RolesManage);
    Get("/users/roles");
    Summary(s => s.Summary = "Get roles for user assignment");
  }
}
