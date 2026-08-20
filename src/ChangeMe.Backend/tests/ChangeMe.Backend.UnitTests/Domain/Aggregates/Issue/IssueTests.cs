using Ardalis.Result;
using ChangeMe.Backend.Domain.Aggregates.Issue;
using ChangeMe.Backend.Domain.Aggregates.Issue.Enums;

namespace ChangeMe.Backend.UnitTests;

public sealed class IssueTests
{
  [Theory]
  [InlineData("  Sample issue  ", "  Needs details  ", "Sample issue", "Needs details")]
  [InlineData("  Another issue", "Description  ", "Another issue", "Description")]
  public void Create_WhenInputContainsWhitespace_ShouldTrimTitleAndDescription(
    string title,
    string description,
    string expectedTitle,
    string expectedDescription)
  {
    var result = Issue.Create(title, description);

    Assert.True(result.IsSuccess);
    Assert.Equal(expectedTitle, result.Value.Title);
    Assert.Equal(expectedDescription, result.Value.Description);
    Assert.Equal(IssueStatus.NEW, result.Value.Status);
    Assert.Equal(IssuePriority.MEDIUM, result.Value.Priority);
  }

  [Theory]
  [InlineData("")]
  [InlineData(" ")]
  [InlineData("ab")]
  public void Create_WhenTitleIsInvalid_ShouldReturnInvalidResult(string title)
  {
    var result = Issue.Create(title, "Valid description");

    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.Invalid, result.Status);
  }

  [Theory]
  [InlineData("")]
  [InlineData(" ")]
  public void Create_WhenDescriptionIsEmpty_ShouldReturnInvalidResult(string description)
  {
    var result = Issue.Create("Valid title", description);

    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.Invalid, result.Status);
  }

  [Theory]
  [InlineData((IssuePriority)999)]
  [InlineData((IssuePriority)(-1))]
  public void Create_WhenPriorityIsInvalid_ShouldReturnInvalidResult(IssuePriority priority)
  {
    var result = Issue.Create("Valid title", "Valid description", priority);

    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.Invalid, result.Status);
  }

  [Theory]
  [InlineData((IssueStatus)999)]
  [InlineData((IssueStatus)(-1))]
  public void Create_WhenStatusIsInvalid_ShouldReturnInvalidResult(IssueStatus status)
  {
    var result = Issue.Create("Valid title", "Valid description", IssuePriority.MEDIUM, status);

    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.Invalid, result.Status);
  }

  [Fact]
  public void RecordCreation_ShouldAddHistoryEntry()
  {
    var actorId = Guid.CreateVersion7();
    var issueResult = Issue.Create("Initial title", "Initial description");

    var result = issueResult.Value.RecordCreation(actorId);

    Assert.True(result.IsSuccess);
    Assert.Single(issueResult.Value.HistoryEntries);
    Assert.Equal(IssueHistoryEventType.ISSUE_CREATED, issueResult.Value.HistoryEntries.Single().EventType);
  }

  [Fact]
  public void UpdateDetails_WhenInputIsValid_ShouldUpdateValuesAndCreateHistoryEntries()
  {
    var actorId = Guid.CreateVersion7();
    var issueResult = Issue.Create("Initial title", "Initial description", IssuePriority.LOW, IssueStatus.NEW);
    issueResult.Value.RecordCreation(actorId);

    var result = issueResult.Value.UpdateDetails(
      "  Updated title  ",
      "  Updated description  ",
      IssuePriority.HIGH,
      IssueStatus.IN_PROGRESS,
      Guid.CreateVersion7(),
      actorId);

    Assert.True(result.IsSuccess);
    Assert.Equal("Updated title", issueResult.Value.Title);
    Assert.Equal("Updated description", issueResult.Value.Description);
    Assert.Equal(IssuePriority.HIGH, issueResult.Value.Priority);
    Assert.Equal(IssueStatus.IN_PROGRESS, issueResult.Value.Status);
    Assert.Equal(6, issueResult.Value.HistoryEntries.Count);
    Assert.Contains(issueResult.Value.HistoryEntries, x => x.EventType == IssueHistoryEventType.ISSUE_CREATED);
    Assert.Contains(issueResult.Value.HistoryEntries, x => x.EventType == IssueHistoryEventType.TITLE_CHANGED);
    Assert.Contains(issueResult.Value.HistoryEntries, x => x.EventType == IssueHistoryEventType.DESCRIPTION_CHANGED);
    Assert.Contains(issueResult.Value.HistoryEntries, x => x.EventType == IssueHistoryEventType.PRIORITY_CHANGED);
    Assert.Contains(issueResult.Value.HistoryEntries, x => x.EventType == IssueHistoryEventType.STATUS_CHANGED);
    Assert.Contains(issueResult.Value.HistoryEntries, x => x.EventType == IssueHistoryEventType.ASSIGNEE_CHANGED);
  }

  [Fact]
  public void UpdateDetails_WhenTitleIsInvalid_ShouldReturnInvalidResultAndKeepPreviousValues()
  {
    var issueResult = Issue.Create("Initial title", "Initial description", IssuePriority.MEDIUM);

    var result = issueResult.Value.UpdateDetails(
      "",
      "Updated description",
      IssuePriority.HIGH,
      IssueStatus.RESOLVED,
      null,
      Guid.CreateVersion7());

    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.Invalid, result.Status);
    Assert.Equal("Initial title", issueResult.Value.Title);
    Assert.Equal("Initial description", issueResult.Value.Description);
    Assert.Equal(IssuePriority.MEDIUM, issueResult.Value.Priority);
    Assert.Equal(IssueStatus.NEW, issueResult.Value.Status);
  }

  [Fact]
  public void AddAcceptanceCriterion_WhenContentIsValid_ShouldAddAcceptanceCriterionToCollection()
  {
    var issueResult = Issue.Create("Valid title", "Valid description");

    var result = issueResult.Value.AddAcceptanceCriterion("First acceptance criterion");

    Assert.True(result.IsSuccess);
    Assert.Single(issueResult.Value.AcceptanceCriteria);
    Assert.Equal("First acceptance criterion", issueResult.Value.AcceptanceCriteria.Single().Content);
  }

  [Fact]
  public void AddComment_WhenContentIsValid_ShouldAddCommentWithoutCreatingHistoryEntry()
  {
    var issueResult = Issue.Create("Valid title", "Valid description");
    var result = issueResult.Value.AddComment("  First comment  ");

    Assert.True(result.IsSuccess);
    Assert.Single(issueResult.Value.Comments);
    Assert.Equal("First comment", issueResult.Value.Comments.Single().Content);
    Assert.Empty(issueResult.Value.HistoryEntries);
  }

  [Fact]
  public void StartWatching_WhenUserAlreadyWatchesIssue_ShouldReturnConflict()
  {
    var issueResult = Issue.Create("Valid title", "Valid description");
    var userId = Guid.CreateVersion7();

    var firstResult = issueResult.Value.StartWatching(userId);
    var secondResult = issueResult.Value.StartWatching(userId);

    Assert.True(firstResult.IsSuccess);
    Assert.False(secondResult.IsSuccess);
    Assert.Equal(ResultStatus.Conflict, secondResult.Status);
  }

  [Fact]
  public void StopWatching_WhenWatcherExists_ShouldRemoveWatcher()
  {
    var issueResult = Issue.Create("Valid title", "Valid description");
    var userId = Guid.CreateVersion7();
    issueResult.Value.StartWatching(userId);

    var result = issueResult.Value.StopWatching(userId);

    Assert.True(result.IsSuccess);
    Assert.Empty(issueResult.Value.Watchers);
  }

  [Fact]
  public void UpdateAcceptanceCriterion_WhenAcceptanceCriterionDoesNotExist_ShouldReturnNotFound()
  {
    var issueResult = Issue.Create("Valid title", "Valid description");

    var result = issueResult.Value.UpdateAcceptanceCriterion(Guid.NewGuid(), "Updated acceptance criterion");

    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.NotFound, result.Status);
  }

  [Fact]
  public void RemoveAcceptanceCriterion_WhenAcceptanceCriterionDoesNotExist_ShouldReturnNotFound()
  {
    var issueResult = Issue.Create("Valid title", "Valid description");

    var result = issueResult.Value.RemoveAcceptanceCriterion(Guid.NewGuid());

    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.NotFound, result.Status);
  }

  [Fact]
  public void AddAcceptanceCriterion_WithActor_ShouldAddHistoryEntryAndUpdateLastActivity()
  {
    var actorId = Guid.CreateVersion7();
    var issueResult = Issue.Create("Valid title", "Valid description");
    var initialLastActivityAt = issueResult.Value.LastActivityAt;

    var result = issueResult.Value.AddAcceptanceCriterion("First acceptance criterion", actorId);

    Assert.True(result.IsSuccess);
    Assert.Contains(issueResult.Value.HistoryEntries, x =>
      x.EventType == IssueHistoryEventType.ACCEPTANCE_CRITERION_ADDED
      && x.CurrentValue == "First acceptance criterion");
    Assert.True(issueResult.Value.LastActivityAt >= initialLastActivityAt);
  }

  [Fact]
  public void UpdateAcceptanceCriterion_WithActor_ShouldAddHistoryEntryWhenContentChanges()
  {
    var actorId = Guid.CreateVersion7();
    var issueResult = Issue.Create("Valid title", "Valid description");
    var criterion = issueResult.Value.AddAcceptanceCriterion("Initial criterion").Value;

    var result = issueResult.Value.UpdateAcceptanceCriterion(criterion.Id, "Updated criterion", actorId);

    Assert.True(result.IsSuccess);
    Assert.Contains(issueResult.Value.HistoryEntries, x =>
      x.EventType == IssueHistoryEventType.ACCEPTANCE_CRITERION_UPDATED
      && x.PreviousValue == "Initial criterion"
      && x.CurrentValue == "Updated criterion");
  }

  [Fact]
  public void RemoveAcceptanceCriterion_WithActor_ShouldAddHistoryEntry()
  {
    var actorId = Guid.CreateVersion7();
    var issueResult = Issue.Create("Valid title", "Valid description");
    var criterion = issueResult.Value.AddAcceptanceCriterion("Criterion to remove").Value;

    var result = issueResult.Value.RemoveAcceptanceCriterion(criterion.Id, actorId);

    Assert.True(result.IsSuccess);
    Assert.Contains(issueResult.Value.HistoryEntries, x =>
      x.EventType == IssueHistoryEventType.ACCEPTANCE_CRITERION_REMOVED
      && x.PreviousValue == "Criterion to remove");
  }
}
