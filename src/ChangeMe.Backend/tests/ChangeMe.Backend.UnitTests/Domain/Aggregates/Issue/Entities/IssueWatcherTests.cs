using Ardalis.Result;
using ChangeMe.Backend.Domain.Aggregates.Issue.Entities;

namespace ChangeMe.Backend.UnitTests;

public sealed class IssueWatcherTests
{
  [Fact]
  public void Create_WhenInputIsValid_ShouldReturnWatcher()
  {
    var issueId = Guid.NewGuid();
    var userId = Guid.NewGuid();

    var result = IssueWatcher.Create(issueId, userId);

    Assert.True(result.IsSuccess);
    Assert.Equal(issueId, result.Value.IssueId);
    Assert.Equal(userId, result.Value.UserId);
  }

  [Fact]
  public void Create_WhenIssueIdIsEmpty_ShouldReturnInvalidResult()
  {
    var result = IssueWatcher.Create(Guid.Empty, Guid.NewGuid());

    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.Invalid, result.Status);
    Assert.Contains(result.ValidationErrors, error => error.Identifier == nameof(IssueWatcher.IssueId));
  }

  [Fact]
  public void Create_WhenUserIdIsEmpty_ShouldReturnInvalidResult()
  {
    var result = IssueWatcher.Create(Guid.NewGuid(), Guid.Empty);

    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.Invalid, result.Status);
    Assert.Contains(result.ValidationErrors, error => error.Identifier == nameof(IssueWatcher.UserId));
  }
}
