using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace Template.Backend.Infrastructure.Email;

public sealed class EmailOptionsValidator : IValidateOptions<EmailOptions>
{
  public ValidateOptionsResult Validate(string? name, EmailOptions options)
  {
    var failures = new List<string>();

    if (string.IsNullOrWhiteSpace(options.Host))
      failures.Add("EmailOptions.Host is required.");

    if (options.Port is < 1 or > 65535)
      failures.Add("EmailOptions.Port must be between 1 and 65535.");

    if (string.IsNullOrWhiteSpace(options.FromEmail))
    {
      failures.Add("EmailOptions.FromEmail is required.");
    }
    else if (!IsValidEmail(options.FromEmail))
    {
      failures.Add("EmailOptions.FromEmail must be a valid email address.");
    }

    return failures.Count == 0
      ? ValidateOptionsResult.Success
      : ValidateOptionsResult.Fail(failures);
  }

  private static bool IsValidEmail(string value)
  {
    try
    {
      _ = new MailAddress(value);
      return true;
    }
    catch (FormatException)
    {
      return false;
    }
  }
}
