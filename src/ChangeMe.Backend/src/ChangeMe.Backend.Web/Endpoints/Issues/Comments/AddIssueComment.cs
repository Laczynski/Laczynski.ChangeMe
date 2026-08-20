using ChangeMe.Backend.Domain.Aggregates.Issue.Entities;
using ChangeMe.Backend.UseCases.Issues.Comments;
using ChangeMe.Backend.UseCases.Issues.Dtos;

namespace ChangeMe.Backend.Web.Endpoints.Issues.Comments;

public class AddIssueComment(IMediator mediator) : BaseEndpoint<AddIssueCommentCommand, IssueDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/issues/{issueId}/comments");
    Summary(s =>
    {
      s.Summary = "Add issue comment";
      s.Description = "Adds a comment to an issue";
    });
  }
}

public sealed class AddIssueCommentCommandValidator : Validator<AddIssueCommentCommand>
{
  public AddIssueCommentCommandValidator()
  {
    RuleFor(x => x.IssueId)
      .NotEmpty();

    RuleFor(x => x.Content)
      .NotEmpty()
      .MaximumLength(IssueCommentConstraints.CONTENT_MAX_LENGTH);
  }
}
