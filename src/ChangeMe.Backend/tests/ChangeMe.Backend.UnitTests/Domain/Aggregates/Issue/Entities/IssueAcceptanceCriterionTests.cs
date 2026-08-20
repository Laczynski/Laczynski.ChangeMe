using Ardalis.Result;
using ChangeMe.Backend.Domain.Aggregates.Issue.Entities;

namespace ChangeMe.Backend.UnitTests;

public sealed class IssueAcceptanceCriterionTests
{
  [Fact]
  public void Create_WhenIssueIdIsEmpty_ShouldReturnInvalidResult()
  {
    var result = IssueAcceptanceCriterion.Create(Guid.Empty, "Acceptance criterion");

    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.Invalid, result.Status);
    Assert.Contains(result.ValidationErrors, error => error.Identifier == "issueId");
  }

  [Theory]
  [InlineData("")]
  [InlineData(" ")]
  public void Create_WhenContentIsEmpty_ShouldReturnInvalidResult(string content)
  {
    var result = IssueAcceptanceCriterion.Create(Guid.NewGuid(), content);

    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.Invalid, result.Status);
  }

  [Fact]
  public void Create_WhenContentExceedsMaxLength_ShouldReturnInvalidResult()
  {
    var result = IssueAcceptanceCriterion.Create(
      Guid.NewGuid(),
      new string('A', IssueAcceptanceCriterionConstraints.CONTENT_MAX_LENGTH + 1));

    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.Invalid, result.Status);
    Assert.Contains(result.ValidationErrors, error => error.Identifier == "content");
  }

  [Theory]
  [InlineData("  Acceptance criterion body  ", "Acceptance criterion body")]
  [InlineData("Acceptance criterion", "Acceptance criterion")]
  public void UpdateContent_WhenContentIsValid_ShouldTrimAndUpdateValue(string content, string expected)
  {
    var acceptanceCriterionResult = IssueAcceptanceCriterion.Create(Guid.NewGuid(), "Initial acceptance criterion");

    var result = acceptanceCriterionResult.Value.UpdateContent(content);

    Assert.True(result.IsSuccess);
    Assert.Equal(expected, acceptanceCriterionResult.Value.Content);
  }
}
