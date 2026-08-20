using Ardalis.Result;
using Bogus;
using ChangeMe.Backend.DataGenerator.Options;
using ChangeMe.Backend.DataGenerator.Persistence;
using ChangeMe.Backend.Domain.Aggregates.Issue.Entities;
using ChangeMe.Backend.Domain.Aggregates.Issue.Enums;
using Microsoft.Extensions.Options;
using DomainIssue = global::ChangeMe.Backend.Domain.Aggregates.Issue.Issue;
using DomainUser = global::ChangeMe.Backend.Domain.Aggregates.Users.User;
using IssueConstraints = global::ChangeMe.Backend.Domain.Aggregates.Issue.IssueConstraints;

namespace ChangeMe.Backend.DataGenerator.Generators;

internal sealed class IssuesGenerator(IOptions<DataGeneratorOptions> options)
{
  public IReadOnlyList<DomainIssue> Generate(IReadOnlyList<DomainUser> demoUsers)
  {
    var config = options.Value;
    var faker = new Faker { Random = new Randomizer(config.Seed + 1) };
    var issues = new List<DomainIssue>(config.Issues);
    var statuses = Enum.GetValues<IssueStatus>();
    var priorities = Enum.GetValues<IssuePriority>();

    for (var index = 0; index < config.Issues; index++)
      issues.Add(CreateDemoIssue(index, demoUsers, faker, statuses, priorities, config));

    return issues;
  }

  private static DomainIssue CreateDemoIssue(
    int index,
    IReadOnlyList<DomainUser> demoUsers,
    Faker faker,
    IssueStatus[] statuses,
    IssuePriority[] priorities,
    DataGeneratorOptions config)
  {
    var actor = PickRandom(demoUsers, faker);
    var issue = CreateIssue(index, demoUsers, faker, statuses, priorities, actor);

    AddAcceptanceCriteria(issue, actor, faker);
    AddComments(issue, demoUsers, faker, config);
    MaybeAddWatcher(issue, demoUsers, faker);
    ApplyDemoAudit(issue, actor);

    return issue;
  }

  private static DomainIssue CreateIssue(
    int index,
    IReadOnlyList<DomainUser> demoUsers,
    Faker faker,
    IssueStatus[] statuses,
    IssuePriority[] priorities,
    DomainUser actor)
  {
    var assignee = faker.Random.Bool(0.7f) ? PickRandom(demoUsers, faker) : null;
    var title = EnsureMinimumTitleLength(
      Truncate(faker.Hacker.Phrase(), IssueConstraints.TITLE_MAX_LENGTH),
      index);
    var description = Truncate(faker.Lorem.Paragraphs(1, 2), IssueConstraints.DESCRIPTION_MAX_LENGTH);

    var issue = EnsureSuccess(
      DomainIssue.Create(
        title,
        description,
        faker.PickRandom(priorities),
        faker.PickRandom(statuses),
        assignee?.Id),
      "Failed to create demo issue");

    issue.RecordCreation(actor.Id);
    return issue;
  }

  private static void AddAcceptanceCriteria(DomainIssue issue, DomainUser actor, Faker faker)
  {
    var criteriaCount = faker.Random.Int(0, 3);
    for (var c = 0; c < criteriaCount; c++)
    {
      var criterion = Truncate(faker.Lorem.Sentence(4), IssueAcceptanceCriterionConstraints.CONTENT_MAX_LENGTH);
      EnsureSuccess(
        issue.AddAcceptanceCriterion(criterion, actor.Id),
        "Failed to add acceptance criterion");
    }
  }

  private static void AddComments(
    DomainIssue issue,
    IReadOnlyList<DomainUser> demoUsers,
    Faker faker,
    DataGeneratorOptions config)
  {
    var commentCount = faker.Random.Int(config.CommentsPerIssueMin, config.CommentsPerIssueMax);
    for (var c = 0; c < commentCount; c++)
    {
      var commentAuthor = PickRandom(demoUsers, faker);
      var content = Truncate(faker.Lorem.Paragraph(), IssueCommentConstraints.CONTENT_MAX_LENGTH);
      var comment = EnsureSuccess(issue.AddComment(content), "Failed to add comment");
      EntityAudit.Apply(comment, commentAuthor.Id);
    }
  }

  private static void MaybeAddWatcher(DomainIssue issue, IReadOnlyList<DomainUser> demoUsers, Faker faker)
  {
    if (!faker.Random.Bool(0.4f))
      return;

    var watcher = PickRandom(demoUsers, faker);
    issue.StartWatching(watcher.Id);
  }

  private static void ApplyDemoAudit(DomainIssue issue, DomainUser actor)
  {
    EntityAudit.Apply(issue, actor.Id);
    EntityAudit.Apply(issue.HistoryEntries, actor.Id);
    EntityAudit.Apply(issue.AcceptanceCriteria, actor.Id);
    EntityAudit.Apply(issue.Comments, actor.Id);
    EntityAudit.Apply(issue.Watchers, actor.Id);
  }

  private static string EnsureMinimumTitleLength(string title, int index) =>
    title.Length < IssueConstraints.TITLE_MIN_LENGTH ? $"Issue {index + 1}: {title}" : title;

  private static T EnsureSuccess<T>(Result<T> result, string action)
  {
    if (result.IsSuccess)
      return result.Value;

    throw new InvalidOperationException(
      $"{action}: {string.Join(", ", result.ValidationErrors.Select(e => e.ErrorMessage))}");
  }

  private static T PickRandom<T>(IReadOnlyList<T> items, Faker faker) =>
    items[faker.Random.Int(0, items.Count - 1)];

  private static string Truncate(string value, int maxLength)
  {
    var trimmed = value.Trim();
    return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
  }
}
