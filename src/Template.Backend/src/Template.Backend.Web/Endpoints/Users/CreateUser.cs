using Template.Backend.UseCases.Users;
using Template.Backend.UseCases.Users.Dtos;
using Template.Backend.Web.Validation;

namespace Template.Backend.Web.Endpoints.Users;

public class CreateUser(IMediator mediator) : BaseEndpoint<CreateUserCommand, UserDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermissions(PermissionCodes.UsersManage, PermissionCodes.RolesManage);
    Post("/users");
    Summary(s => s.Summary = "Create user");
  }
}

public sealed class CreateUserCommandValidator : Validator<CreateUserCommand>
{
  public CreateUserCommandValidator(IPasswordPolicyValidator passwordPolicyValidator)
  {
    RuleFor(x => x.FirstName)
      .NotEmpty()
      .MaximumLength(UserConstraints.NAME_MAX_LENGTH);

    RuleFor(x => x.LastName)
      .NotEmpty()
      .MaximumLength(UserConstraints.NAME_MAX_LENGTH);

    RuleFor(x => x.Email)
      .NotEmpty()
      .EmailAddress()
      .MaximumLength(UserConstraints.EMAIL_MAX_LENGTH);

    RuleFor(x => x.Password)
      .NotEmpty()
      .MustSatisfyPasswordPolicy(passwordPolicyValidator);

    RuleFor(x => x.RoleIds).NotEmpty();
  }
}
