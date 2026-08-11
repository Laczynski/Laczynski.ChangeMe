using DataGrid.Abstractions;
using Template.Backend.UseCases.Users.Dtos;
using Template.Backend.UseCases.Users.Sessions;

namespace Template.Backend.Web.Endpoints.Users.Sessions;

public class GetUserSessions(IMediator mediator)
  : BaseEndpoint<GetUserSessionsQuery, GridResult<AdminUserSessionDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.SessionsViewAny);
    Get("/users/{Id}/sessions");
    Summary(s => s.Summary = "Get user active sessions");
  }
}
