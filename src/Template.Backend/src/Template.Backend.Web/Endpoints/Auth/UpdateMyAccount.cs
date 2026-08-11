using Template.Backend.UseCases.Auth;
using Template.Backend.UseCases.Auth.Dtos;

namespace Template.Backend.Web.Endpoints.Auth;

public class UpdateMyAccount(IMediator mediator) : BaseEndpoint<UpdateMyAccountCommand, MyAccountDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Put("/auth/account");
    Summary(s =>
    {
      s.Summary = "Update my account";
      s.Description = "Updates the signed-in user's profile.";
    });
  }
}

public sealed class UpdateMyAccountCommandValidator : Validator<UpdateMyAccountCommand>
{
  public UpdateMyAccountCommandValidator()
  {
    RuleFor(x => x.Version).GreaterThanOrEqualTo(0);

    RuleFor(x => x.FirstName)
      .NotEmpty()
      .MaximumLength(UserConstraints.NAME_MAX_LENGTH);

    RuleFor(x => x.LastName)
      .NotEmpty()
      .MaximumLength(UserConstraints.NAME_MAX_LENGTH);
  }
}
