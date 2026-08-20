using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.Infrastructure.Persistence;

public sealed class InitialAdministratorOptionsValidator(IOptions<AuthOptions> authOptions)
  : IValidateOptions<InitialAdministratorOptions>
{
  public ValidateOptionsResult Validate(string? name, InitialAdministratorOptions options)
  {
    var values = new Dictionary<string, string?>
    {
      [nameof(options.Email)] = options.Email,
      [nameof(options.Password)] = options.Password,
      [nameof(options.FirstName)] = options.FirstName,
      [nameof(options.LastName)] = options.LastName
    };

    if (values.Values.All(string.IsNullOrWhiteSpace))
      return ValidateOptionsResult.Success;

    var failures = values
      .Where(pair => string.IsNullOrWhiteSpace(pair.Value))
      .Select(pair => $"InitialAdministratorOptions.{pair.Key} is required when administrator bootstrap is configured.")
      .ToList();

    if (!string.IsNullOrWhiteSpace(options.Email))
    {
      if (options.Email.Trim().Length > UserConstraints.EMAIL_MAX_LENGTH)
      {
        failures.Add(
          $"InitialAdministratorOptions.Email must not exceed {UserConstraints.EMAIL_MAX_LENGTH} characters.");
      }
      else if (!EmailAddressValidation.IsValid(options.Email))
      {
        failures.Add("InitialAdministratorOptions.Email must be a valid email address.");
      }
    }

    ValidateName(options.FirstName, nameof(options.FirstName), failures);
    ValidateName(options.LastName, nameof(options.LastName), failures);

    if (!string.IsNullOrWhiteSpace(options.Password))
    {
      failures.AddRange(
        PasswordPolicyValidator
          .Validate(options.Password, authOptions.Value.PasswordPolicy, nameof(options.Password))
          .Select(error => $"InitialAdministratorOptions.Password: {error.ErrorMessage}"));
    }

    return failures.Count == 0
      ? ValidateOptionsResult.Success
      : ValidateOptionsResult.Fail(failures);
  }

  private static void ValidateName(string? value, string propertyName, List<string> failures)
  {
    if (string.IsNullOrWhiteSpace(value))
      return;

    if (value.Trim().Length > UserConstraints.NAME_MAX_LENGTH)
    {
      failures.Add(
        $"InitialAdministratorOptions.{propertyName} must not exceed {UserConstraints.NAME_MAX_LENGTH} characters.");
    }
  }
}
