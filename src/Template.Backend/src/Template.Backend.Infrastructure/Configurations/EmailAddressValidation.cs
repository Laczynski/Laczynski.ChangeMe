using System.Net.Mail;

namespace Template.Backend.Infrastructure.Configurations;

public static class EmailAddressValidation
{
  public static bool IsValid(string? value)
  {
    return !string.IsNullOrWhiteSpace(value) && MailAddress.TryCreate(value, out _);
  }
}
