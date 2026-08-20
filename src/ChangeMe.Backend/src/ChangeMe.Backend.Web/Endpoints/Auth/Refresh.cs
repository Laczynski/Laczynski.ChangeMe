using ChangeMe.Backend.UseCases.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using ChangeMe.Backend.Web.Configurations;

namespace ChangeMe.Backend.Web.Endpoints.Auth;

public class Refresh(IMediator mediator) : BaseEndpoint<RefreshSessionCommand, AuthResponseDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/auth/refresh");
    Options(x => x.RequireRateLimiting(RateLimitingConfig.AuthPolicyName));
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Refresh session";
      s.Description = "Renew short-lived credentials using a refresh token.";
    });
  }
}

public sealed class RefreshSessionCommandValidator : Validator<RefreshSessionCommand>
{
  public RefreshSessionCommandValidator()
  {
    RuleFor(x => x.RefreshToken)
      .NotEmpty();
  }
}
