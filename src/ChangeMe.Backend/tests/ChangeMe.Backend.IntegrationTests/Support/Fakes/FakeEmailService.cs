using System.Collections.Concurrent;
using Ardalis.Result;

namespace ChangeMe.Backend.IntegrationTests.Support.Fakes;

internal sealed class FakeEmailService : IEmailService
{
  private readonly ConcurrentQueue<FakeEmailMessage> sentEmails = new();

  public IReadOnlyCollection<FakeEmailMessage> SentEmails => sentEmails.ToArray();

  public Task<Result> SendEmailAsync(string to, string subject, string body)
  {
    sentEmails.Enqueue(new FakeEmailMessage([to], subject, body));
    return Task.FromResult(Result.Success());
  }

  public Task<Result> SendEmailToManyAsync(IEnumerable<string> recipients, string subject, string body)
  {
    sentEmails.Enqueue(new FakeEmailMessage(recipients.ToArray(), subject, body));
    return Task.FromResult(Result.Success());
  }

  public void Clear()
  {
    while (sentEmails.TryDequeue(out _))
    {
    }
  }
}

internal sealed record FakeEmailMessage(
  IReadOnlyCollection<string> Recipients,
  string Subject,
  string Body);
