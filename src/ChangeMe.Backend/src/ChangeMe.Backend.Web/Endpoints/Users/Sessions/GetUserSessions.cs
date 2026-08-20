using ChangeMe.Backend.UseCases.Users.Dtos;
using ChangeMe.Backend.UseCases.Users.Sessions;
using DataGrid.Abstractions;

namespace ChangeMe.Backend.Web.Endpoints.Users.Sessions;

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
