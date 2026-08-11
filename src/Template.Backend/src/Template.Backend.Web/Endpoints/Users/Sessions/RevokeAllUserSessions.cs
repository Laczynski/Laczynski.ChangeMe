using Template.Backend.UseCases.Users.Sessions;

namespace Template.Backend.Web.Endpoints.Users.Sessions;

public class RevokeAllUserSessions(IMediator mediator) : BaseEndpoint<RevokeAllUserSessionsCommand, bool>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.SessionsManageAny);
    Post("/users/{Id}/sessions/revoke-all");
    Summary(s => s.Summary = "Revoke all user sessions");
  }
}
