using System.Text;
using Microsoft.Extensions.Options;

namespace Template.Backend.Infrastructure.Auth;

public sealed class AuthOptionsValidator : IValidateOptions<AuthOptions>
{
  private const int MinimumSigningKeyBytes = 32;

  public ValidateOptionsResult Validate(string? name, AuthOptions options)
  {
    var failures = new List<string>();

    if (string.IsNullOrWhiteSpace(options.Jwt.Issuer))
      failures.Add("AuthOptions.Jwt.Issuer is required.");

    if (string.IsNullOrWhiteSpace(options.Jwt.Audience))
      failures.Add("AuthOptions.Jwt.Audience is required.");

    if (Encoding.UTF8.GetByteCount(options.Jwt.SigningKey ?? string.Empty) < MinimumSigningKeyBytes)
    {
      failures.Add(
        $"AuthOptions.Jwt.SigningKey must contain at least {MinimumSigningKeyBytes} bytes.");
    }

    if (options.Jwt.ExpirationMinutes <= 0)
      failures.Add("AuthOptions.Jwt.ExpirationMinutes must be greater than zero.");

    if (options.Jwt.SessionLifetimeDays <= 0)
      failures.Add("AuthOptions.Jwt.SessionLifetimeDays must be greater than zero.");

    if (!IsAbsoluteHttpUrl(options.FrontendBaseUrl))
      failures.Add("AuthOptions.FrontendBaseUrl must be an absolute HTTP or HTTPS URL.");

    if (options.PasswordPolicy.MinimumLength <= 0)
      failures.Add("AuthOptions.PasswordPolicy.MinimumLength must be greater than zero.");

    if (options.PasswordPolicy.MaximumLength <= 0)
      failures.Add("AuthOptions.PasswordPolicy.MaximumLength must be greater than zero.");

    if (options.PasswordPolicy.MinimumLength > options.PasswordPolicy.MaximumLength)
    {
      failures.Add(
        "AuthOptions.PasswordPolicy.MinimumLength must not exceed AuthOptions.PasswordPolicy.MaximumLength.");
    }

    return failures.Count == 0
      ? ValidateOptionsResult.Success
      : ValidateOptionsResult.Fail(failures);
  }

  private static bool IsAbsoluteHttpUrl(string? value)
  {
    return Uri.TryCreate(value, UriKind.Absolute, out var uri)
      && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
      && !string.IsNullOrWhiteSpace(uri.Host);
  }
}
