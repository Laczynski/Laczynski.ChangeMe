using ChangeMe.Backend.UseCases.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using ChangeMe.Backend.Web.Configurations;

namespace ChangeMe.Backend.Web.Endpoints.Auth;

public class Login(IMediator mediator) : BaseEndpoint<LoginUserCommand, LoginResponseDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/auth/login");
    Options(x => x.RequireRateLimiting(RateLimitingConfig.AuthPolicyName));
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Login user";
      s.Description = "Authenticate user and return JWT token.";
    });
  }
}

public sealed class LoginUserCommandValidator : Validator<LoginUserCommand>
{
  public LoginUserCommandValidator()
  {
    RuleFor(x => x.Email)
      .NotEmpty()
      .EmailAddress()
      .MaximumLength(UserConstraints.EMAIL_MAX_LENGTH);

    RuleFor(x => x.Password)
      .NotEmpty()
      .MinimumLength(UserConstraints.PASSWORD_MIN_LENGTH)
      .MaximumLength(UserConstraints.PASSWORD_MAX_LENGTH);
  }
}
