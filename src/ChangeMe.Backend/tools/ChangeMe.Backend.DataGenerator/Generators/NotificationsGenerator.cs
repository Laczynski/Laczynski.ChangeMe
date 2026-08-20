using Bogus;
using ChangeMe.Backend.DataGenerator.Options;
using ChangeMe.Backend.DataGenerator.Persistence;
using ChangeMe.Backend.Domain.Aggregates.Notifications;
using ChangeMe.Backend.Domain.Aggregates.Notifications.Enums;
using Microsoft.Extensions.Options;
using DomainIssue = global::ChangeMe.Backend.Domain.Aggregates.Issue.Issue;
using DomainUser = global::ChangeMe.Backend.Domain.Aggregates.Users.User;

namespace ChangeMe.Backend.DataGenerator.Generators;

internal sealed class NotificationsGenerator(IOptions<DataGeneratorOptions> options)
{
  public IReadOnlyList<Notification> Generate(
    IReadOnlyList<DomainUser> demoUsers,
    IReadOnlyList<DomainIssue> issues)
  {
    var config = options.Value;
    var faker = new Faker { Random = new Randomizer(config.Seed + 2) };
    var eventTypes = Enum.GetValues<NotificationEventType>();
    var notifications = new List<Notification>();

    foreach (var user in demoUsers)
    {
      var count = faker.Random.Int(config.NotificationsPerUserMin, config.NotificationsPerUserMax);
      for (var i = 0; i < count; i++)
      {
        var issue = issues[faker.Random.Int(0, issues.Count - 1)];
        var historyEntry = issue.HistoryEntries.FirstOrDefault();
        if (historyEntry is null)
          continue;

        var recipient = demoUsers[faker.Random.Int(0, demoUsers.Count - 1)];
        var eventType = faker.PickRandom(eventTypes);
        var link = $"/issues/{issue.Id}";

        var notificationResult = Notification.Create(
          recipient.Id,
          issue.Id,
          historyEntry.Id,
          eventType,
          issue.Title,
          $"Demo notification for {issue.Title}",
          link);

        if (!notificationResult.IsSuccess)
          throw new InvalidOperationException($"Failed to create notification: {string.Join(", ", notificationResult.ValidationErrors.Select(e => e.ErrorMessage))}");

        var notification = notificationResult.Value;
        if (faker.Random.Bool(0.3f))
          notification.MarkAsRead();

        EntityAudit.Apply(notification, recipient.Id);
        notifications.Add(notification);
      }
    }

    return notifications;
  }
}
