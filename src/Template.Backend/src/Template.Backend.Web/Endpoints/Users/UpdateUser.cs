using Template.Backend.UseCases.Users;
using Template.Backend.UseCases.Users.Dtos;

namespace Template.Backend.Web.Endpoints.Users;

public class UpdateUser(IMediator mediator) : BaseEndpoint<UpdateUserCommand, UserDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.UsersManage);
    Put("/users/{Id}");
    Summary(s => s.Summary = "Update user");
  }
}

public sealed class UpdateUserCommandValidator : Validator<UpdateUserCommand>
{
  public UpdateUserCommandValidator()
  {
    RuleFor(x => x.Id).NotEmpty();
    RuleFor(x => x.Version).GreaterThanOrEqualTo(0);
    RuleFor(x => x.FirstName)
      .MaximumLength(UserConstraints.NAME_MAX_LENGTH)
      .When(x => !string.IsNullOrWhiteSpace(x.FirstName));

    RuleFor(x => x.LastName)
      .MaximumLength(UserConstraints.NAME_MAX_LENGTH)
      .When(x => !string.IsNullOrWhiteSpace(x.LastName));

    RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(UserConstraints.EMAIL_MAX_LENGTH);
  }
}
