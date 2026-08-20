using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.Infrastructure.Email;

public class EmailService(IOptions<EmailOptions> settings, ILogger<EmailService> logger) : IEmailService
{
  private readonly EmailOptions _settings = settings.Value;

  public async Task<Result> SendEmailAsync(string to, string subject, string body)
  {
    try
    {
      var message = CreateMailMessage([to], subject, body);
      await SendMailAsync(message);
      logger.LogInformation("Email sent successfully to {Recipient}", to);
      return Result.Success();
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Failed to send email to {Recipient}: {Error}", to, ex.Message);
      return Result.Error();
    }
  }

  public async Task<Result> SendEmailToManyAsync(IEnumerable<string> recipients, string subject, string body)
  {
    try
    {
      var message = CreateMailMessage(recipients, subject, body);
      await SendMailAsync(message);
      logger.LogInformation("Email sent successfully to multiple recipients");
      return Result.Success();
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Failed to send email to multiple recipients: {Error}", ex.Message);
      return Result.Error();
    }
  }

  private MailMessage CreateMailMessage(IEnumerable<string> recipients, string subject, string body)
  {
    var message = new MailMessage
    {
      From = new MailAddress(_settings.FromEmail, _settings.FromName),
      Subject = subject,
      Body = body,
      IsBodyHtml = true
    };

    foreach (var recipient in recipients)
    {
      message.To.Add(recipient);
    }

    return message;
  }

  private async Task SendMailAsync(MailMessage message)
  {
    using var client = new SmtpClient(_settings.Host, _settings.Port)
    {
      EnableSsl = _settings.EnableSsl,
      Credentials = new NetworkCredential(_settings.Username, _settings.Password)
    };

    await client.SendMailAsync(message);
  }
}
