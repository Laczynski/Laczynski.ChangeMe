using Microsoft.Extensions.Options;

namespace Template.Backend.Infrastructure.Auth;

public sealed class PasswordPolicyValidator(IOptions<AuthOptions> authOptions) : IPasswordPolicyValidator
{
  public IReadOnlyList<ValidationError> Validate(string password, string propertyName = "Password")
  {
    var policy = authOptions.Value.PasswordPolicy;
    var errors = new List<ValidationError>();

    if (string.IsNullOrEmpty(password))
      return errors;

    if (password.Length < policy.MinimumLength)
    {
      errors.Add(new ValidationError(
        propertyName,
        $"Password must be at least {policy.MinimumLength} characters."));
    }

    if (password.Length > policy.MaximumLength)
    {
      errors.Add(new ValidationError(
        propertyName,
        $"Password must not exceed {policy.MaximumLength} characters."));
    }

    if (policy.RequireUppercase && !password.Any(char.IsUpper))
    {
      errors.Add(new ValidationError(
        propertyName,
        "Password must contain at least one uppercase letter."));
    }

    if (policy.RequireLowercase && !password.Any(char.IsLower))
    {
      errors.Add(new ValidationError(
        propertyName,
        "Password must contain at least one lowercase letter."));
    }

    if (policy.RequireDigit && !password.Any(char.IsDigit))
    {
      errors.Add(new ValidationError(
        propertyName,
        "Password must contain at least one digit."));
    }

    if (policy.RequireSpecialCharacter && !password.Any(c => !char.IsLetterOrDigit(c)))
    {
      errors.Add(new ValidationError(
        propertyName,
        "Password must contain at least one special character."));
    }

    return errors;
  }
}
