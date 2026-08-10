using DataGrid.Abstractions;
using Template.Backend.UseCases.Auth.Dtos;
using Template.Backend.UseCases.Auth.Sessions;

namespace Template.Backend.Web.Endpoints.Auth.Sessions;

public class GetMySessions(IMediator mediator) : BaseEndpoint<GetMySessionsQuery, GridResult<UserSessionDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.SessionsViewOwn);
    Get("/auth/sessions");
    Summary(s =>
    {
      s.Summary = "List my sessions";
      s.Description = "Returns active sessions for the signed-in user.";
    });
  }
}
