using ChangeMe.Backend.UseCases.Auth.Dtos;
using ChangeMe.Backend.UseCases.Auth.Sessions;
using QueryGrid.Abstractions;

namespace ChangeMe.Backend.Web.Endpoints.Auth.Sessions;

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
