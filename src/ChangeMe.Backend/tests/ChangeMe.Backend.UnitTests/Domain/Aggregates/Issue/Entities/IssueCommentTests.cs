using Ardalis.Result;
using ChangeMe.Backend.Domain.Aggregates.Issue.Entities;

namespace ChangeMe.Backend.UnitTests;

public sealed class IssueCommentTests
{
  [Fact]
  public void Create_WhenInputIsValid_ShouldTrimContent()
  {
    var result = IssueComment.Create(Guid.NewGuid(), "  Comment body  ");

    Assert.True(result.IsSuccess);
    Assert.Equal("Comment body", result.Value.Content);
  }

  [Fact]
  public void Create_WhenIssueIdIsEmpty_ShouldReturnInvalidResult()
  {
    var result = IssueComment.Create(Guid.Empty, "Comment body");

    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.Invalid, result.Status);
    Assert.Contains(result.ValidationErrors, error => error.Identifier == nameof(IssueComment.IssueId));
  }

  [Theory]
  [InlineData("")]
  [InlineData(" ")]
  public void Create_WhenContentIsEmpty_ShouldReturnInvalidResult(string content)
  {
    var result = IssueComment.Create(Guid.NewGuid(), content);

    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.Invalid, result.Status);
    Assert.Contains(result.ValidationErrors, error => error.Identifier == nameof(IssueComment.Content));
  }

  [Fact]
  public void Create_WhenContentExceedsMaxLength_ShouldReturnInvalidResult()
  {
    var result = IssueComment.Create(
      Guid.NewGuid(),
      new string('C', IssueCommentConstraints.CONTENT_MAX_LENGTH + 1));

    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.Invalid, result.Status);
    Assert.Contains(result.ValidationErrors, error => error.Identifier == nameof(IssueComment.Content));
  }
}
