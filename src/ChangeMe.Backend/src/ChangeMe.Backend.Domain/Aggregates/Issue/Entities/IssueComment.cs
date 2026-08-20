namespace ChangeMe.Backend.Domain.Aggregates.Issue.Entities;

public class IssueComment : Entity
{
  private IssueComment() { }

  public Guid IssueId { get; private set; }
  public string Content { get; private set; } = string.Empty;

  public static Result<IssueComment> Create(Guid issueId, string content)
  {
    var validationErrors = new List<ValidationError>();

    if (issueId == Guid.Empty)
      validationErrors.Add(new ValidationError(nameof(IssueId), "cannot be empty"));

    if (string.IsNullOrWhiteSpace(content))
      validationErrors.Add(new ValidationError(nameof(Content), "cannot be empty"));
    else if (content.Trim().Length > IssueCommentConstraints.CONTENT_MAX_LENGTH)
      validationErrors.Add(new ValidationError(nameof(Content), $"cannot be longer than {IssueCommentConstraints.CONTENT_MAX_LENGTH} characters"));

    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    return Result.Success(new IssueComment
    {
      IssueId = issueId,
      Content = content.Trim(),
    });
  }
}

public static class IssueCommentConstraints
{
  public const int CONTENT_MAX_LENGTH = 4000;
}
