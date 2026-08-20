using ChangeMe.Backend.UseCases.Auth.Sessions;

namespace ChangeMe.Backend.Web.Endpoints.Auth.Sessions;

public class LogoutAll(IMediator mediator) : BaseEndpointWithoutRequest<LogoutAllSessionsCommand, bool>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.SessionsManageOwn);
    Post("/auth/logout-all");
    Summary(s =>
    {
      s.Summary = "Sign out everywhere";
      s.Description = "Revoke all active sessions for the current user.";
    });
  }
}
