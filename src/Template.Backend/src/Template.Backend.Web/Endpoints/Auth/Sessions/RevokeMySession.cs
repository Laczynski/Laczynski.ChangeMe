using Template.Backend.UseCases.Auth.Sessions;

namespace Template.Backend.Web.Endpoints.Auth.Sessions;

public class RevokeMySession(IMediator mediator) : BaseEndpoint<RevokeMySessionCommand, bool>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.SessionsManageOwn);
    Delete("/auth/sessions/{SessionId}");
    Summary(s =>
    {
      s.Summary = "Revoke session";
      s.Description = "Revoke a non-current session for the signed-in user.";
    });
  }
}

public sealed class RevokeMySessionCommandValidator : Validator<RevokeMySessionCommand>
{
  public RevokeMySessionCommandValidator()
  {
    RuleFor(x => x.SessionId)
      .NotEmpty();
  }
}
