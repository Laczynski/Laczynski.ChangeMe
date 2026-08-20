namespace ChangeMe.Backend.Domain.Aggregates.Issue.Entities;

public class IssueWatcher : Entity
{
  private IssueWatcher() { }

  public Guid IssueId { get; private set; }
  public Guid UserId { get; private set; }

  public static Result<IssueWatcher> Create(Guid issueId, Guid userId)
  {
    var validationErrors = new List<ValidationError>();

    if (issueId == Guid.Empty)
      validationErrors.Add(new ValidationError(nameof(IssueId), "cannot be empty"));

    if (userId == Guid.Empty)
      validationErrors.Add(new ValidationError(nameof(UserId), "cannot be empty"));

    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    return Result.Success(new IssueWatcher
    {
      IssueId = issueId,
      UserId = userId,
    });
  }
}
