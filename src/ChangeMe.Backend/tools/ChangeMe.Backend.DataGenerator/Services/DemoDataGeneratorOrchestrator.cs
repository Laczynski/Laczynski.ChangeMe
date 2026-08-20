using ChangeMe.Backend.DataGenerator.Generators;
using ChangeMe.Backend.DataGenerator.Options;
using ChangeMe.Backend.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.DataGenerator.Services;

internal sealed class DemoDataGeneratorOrchestrator(
  ApplicationDbContext dbContext,
  UsersGenerator usersGenerator,
  IssuesGenerator issuesGenerator,
  NotificationsGenerator notificationsGenerator,
  IOptions<DataGeneratorOptions> options,
  ILogger<DemoDataGeneratorOrchestrator> logger)
{
  public async Task<GenerationSummary> GenerateAsync(CancellationToken cancellationToken)
  {
    var config = options.Value;

    var users = await usersGenerator.GenerateAsync(cancellationToken);
    var issues = issuesGenerator.Generate(users);

    dbContext.Issues.AddRange(issues);
    await dbContext.SaveChangesAsync(cancellationToken);

    var notifications = notificationsGenerator.Generate(users, issues);
    if (notifications.Count > 0)
    {
      dbContext.Notifications.AddRange(notifications);
      await dbContext.SaveChangesAsync(cancellationToken);
    }

    var summary = new GenerationSummary(
      users.Count,
      issues.Count,
      issues.Sum(i => i.Comments.Count),
      notifications.Count,
      users.Select(u => u.Email).ToList(),
      config.DefaultPassword);

    logger.LogInformation(
      "Demo data generated: {UserCount} users, {IssueCount} issues, {CommentCount} comments, {NotificationCount} notifications",
      summary.UserCount,
      summary.IssueCount,
      summary.CommentCount,
      summary.NotificationCount);

    foreach (var email in summary.DemoEmails)
      logger.LogInformation("Demo account: {Email} / {Password}", email, summary.DefaultPassword);

    return summary;
  }
}

internal sealed record GenerationSummary(
  int UserCount,
  int IssueCount,
  int CommentCount,
  int NotificationCount,
  IReadOnlyList<string> DemoEmails,
  string DefaultPassword);
