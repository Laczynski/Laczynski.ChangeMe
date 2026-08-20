using Ardalis.Result;
using ChangeMe.Backend.Domain.Aggregates.Issue.Entities;
using ChangeMe.Backend.Domain.Aggregates.Issue.Enums;

namespace ChangeMe.Backend.UnitTests;

public sealed class IssueHistoryEntryTests
{
  [Fact]
  public void Create_WhenInputIsValid_ShouldTrimSummaryAndKeepValues()
  {
    var relatedCommentId = Guid.NewGuid();

    var result = IssueHistoryEntry.Create(
      Guid.NewGuid(),
      IssueHistoryEventType.STATUS_CHANGED,
      Guid.NewGuid(),
      "  Status changed  ",
      "NEW",
      "CLOSED",
      relatedCommentId);

    Assert.True(result.IsSuccess);
    Assert.Equal("Status changed", result.Value.Summary);
    Assert.Equal("NEW", result.Value.PreviousValue);
    Assert.Equal("CLOSED", result.Value.CurrentValue);
    Assert.Equal(relatedCommentId, result.Value.RelatedCommentId);
  }

  [Fact]
  public void Create_WhenRequiredValuesAreInvalid_ShouldReturnInvalidResult()
  {
    var result = IssueHistoryEntry.Create(
      Guid.Empty,
      (IssueHistoryEventType)999,
      Guid.Empty,
      " ");

    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.Invalid, result.Status);
    Assert.Contains(result.ValidationErrors, error => error.Identifier == nameof(IssueHistoryEntry.IssueId));
    Assert.Contains(result.ValidationErrors, error => error.Identifier == nameof(IssueHistoryEntry.ActorUserId));
    Assert.Contains(result.ValidationErrors, error => error.Identifier == nameof(IssueHistoryEntry.EventType));
    Assert.Contains(result.ValidationErrors, error => error.Identifier == nameof(IssueHistoryEntry.Summary));
  }

  [Fact]
  public void Create_WhenSummaryExceedsMaxLength_ShouldReturnInvalidResult()
  {
    var result = IssueHistoryEntry.Create(
      Guid.NewGuid(),
      IssueHistoryEventType.STATUS_CHANGED,
      Guid.NewGuid(),
      new string('S', IssueHistoryConstraints.SUMMARY_MAX_LENGTH + 1));

    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.Invalid, result.Status);
    Assert.Contains(result.ValidationErrors, error => error.Identifier == nameof(IssueHistoryEntry.Summary));
  }

  [Fact]
  public void Create_WhenPreviousValueExceedsMaxLength_ShouldReturnInvalidResult()
  {
    var result = IssueHistoryEntry.Create(
      Guid.NewGuid(),
      IssueHistoryEventType.STATUS_CHANGED,
      Guid.NewGuid(),
      "Summary",
      new string('P', IssueHistoryConstraints.VALUE_MAX_LENGTH + 1));

    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.Invalid, result.Status);
    Assert.Contains(result.ValidationErrors, error => error.Identifier == nameof(IssueHistoryEntry.PreviousValue));
  }

  [Fact]
  public void Create_WhenCurrentValueExceedsMaxLength_ShouldReturnInvalidResult()
  {
    var result = IssueHistoryEntry.Create(
      Guid.NewGuid(),
      IssueHistoryEventType.STATUS_CHANGED,
      Guid.NewGuid(),
      "Summary",
      null,
      new string('C', IssueHistoryConstraints.VALUE_MAX_LENGTH + 1));

    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.Invalid, result.Status);
    Assert.Contains(result.ValidationErrors, error => error.Identifier == nameof(IssueHistoryEntry.CurrentValue));
  }
}
