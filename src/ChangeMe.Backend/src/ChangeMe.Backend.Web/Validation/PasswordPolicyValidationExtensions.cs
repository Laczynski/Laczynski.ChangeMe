namespace ChangeMe.Backend.Web.Validation;

public static class PasswordPolicyValidationExtensions
{
  public static IRuleBuilder<T, string> MustSatisfyPasswordPolicy<T>(
    this IRuleBuilder<T, string> ruleBuilder,
    IPasswordPolicyValidator passwordPolicyValidator)
  {
    ruleBuilder.Custom((password, context) =>
    {
      foreach (var error in passwordPolicyValidator.Validate(password, context.PropertyPath))
        context.AddFailure(error.ErrorMessage);
    });

    return ruleBuilder;
  }
}
