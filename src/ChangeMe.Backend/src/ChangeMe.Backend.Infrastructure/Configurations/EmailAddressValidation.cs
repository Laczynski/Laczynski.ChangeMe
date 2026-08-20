using System.Net.Mail;

namespace ChangeMe.Backend.Infrastructure.Configurations;

public static class EmailAddressValidation
{
  public static bool IsValid(string? value)
  {
    return !string.IsNullOrWhiteSpace(value) && MailAddress.TryCreate(value, out _);
  }
}
