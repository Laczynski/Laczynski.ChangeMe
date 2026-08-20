namespace ChangeMe.Backend.Domain.Aggregates.Issue.Entities;

public class IssueAcceptanceCriterion : Entity
{

  public string Content { get; private set; } = string.Empty;
  public Guid IssueId { get; private set; }

  private IssueAcceptanceCriterion() { }


  public static Result<IssueAcceptanceCriterion> Create(Guid issueId, string content)
  {
    var validationErrors = new List<ValidationError>();
    if (issueId == Guid.Empty)
      validationErrors.Add(new ValidationError("issueId", "cannot be empty"));
    if (string.IsNullOrWhiteSpace(content))
      validationErrors.Add(new ValidationError("content", "cannot be empty"));
    if (content.Length > IssueAcceptanceCriterionConstraints.CONTENT_MAX_LENGTH)
      validationErrors.Add(new ValidationError("content", $"cannot be longer than {IssueAcceptanceCriterionConstraints.CONTENT_MAX_LENGTH} characters"));
    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    var issueAcceptanceCriterion = new IssueAcceptanceCriterion()
    {
      IssueId = issueId,
      Content = content.Trim(),
    };
    return Result.Success(issueAcceptanceCriterion);
  }

  public Result<IssueAcceptanceCriterion> UpdateContent(string content)
  {
    var validationErrors = new List<ValidationError>();
    if (string.IsNullOrWhiteSpace(content))
      validationErrors.Add(new ValidationError("content", "cannot be empty"));
    if (content.Length > IssueAcceptanceCriterionConstraints.CONTENT_MAX_LENGTH)
      validationErrors.Add(new ValidationError("content", $"cannot be longer than {IssueAcceptanceCriterionConstraints.CONTENT_MAX_LENGTH} characters"));
    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    Content = content.Trim();
    return Result.Success(this);
  }

}


public static class IssueAcceptanceCriterionConstraints
{
  public const int CONTENT_MAX_LENGTH = 2000;
}
