using Ardalis.Result;
using ChangeMe.Backend.Infrastructure.Email;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UnitTests;

public sealed class EmailServiceTests
{
  [Fact]
  public async Task SendEmailAsync_WhenFromEmailIsInvalid_ShouldReturnError()
  {
    var service = CreateService(new EmailOptions
    {
      Host = "localhost",
      Port = 25,
      FromEmail = string.Empty,
      FromName = "Tests"
    });

    var result = await service.SendEmailAsync("recipient@example.com", "Subject", "Body");

    Assert.Equal(ResultStatus.Error, result.Status);
  }

  [Fact]
  public async Task SendEmailToManyAsync_WhenFromEmailIsInvalid_ShouldReturnError()
  {
    var service = CreateService(new EmailOptions
    {
      Host = "localhost",
      Port = 25,
      FromEmail = string.Empty,
      FromName = "Tests"
    });

    var result = await service.SendEmailToManyAsync(["one@example.com", "two@example.com"], "Subject", "Body");

    Assert.Equal(ResultStatus.Error, result.Status);
  }

  private static EmailService CreateService(EmailOptions options)
  {
    return new EmailService(Options.Create(options), NullLogger<EmailService>.Instance);
  }
}
