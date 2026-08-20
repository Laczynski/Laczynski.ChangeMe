using ChangeMe.Backend.UseCases.Users.Sessions;

namespace ChangeMe.Backend.Web.Endpoints.Users.Sessions;

public class RevokeUserSession(IMediator mediator) : BaseEndpoint<RevokeUserSessionCommand, bool>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.SessionsManageAny);
    Delete("/users/{Id}/sessions/{SessionId}");
    Summary(s => s.Summary = "Revoke user session");
  }
}
