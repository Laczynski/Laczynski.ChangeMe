namespace ChangeMe.Backend.Domain.Common;

public interface IEmailService
{
  Task<Result> SendEmailAsync(string to, string subject, string body);
  Task<Result> SendEmailToManyAsync(IEnumerable<string> recipients, string subject, string body);
}
