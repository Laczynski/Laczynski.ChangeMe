using Template.Backend.Domain.Aggregates.Issue;
using Template.Backend.Domain.Aggregates.Issue.Entities;
using Template.Backend.UseCases.Issues;
using Template.Backend.UseCases.Issues.Dtos;

namespace Template.Backend.Web.Endpoints.Issues;

public class CreateIssue(IMediator mediator) : BaseEndpoint<CreateIssueCommand, IssueDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/issues");
    Summary(s =>
    {
      s.Summary = "Create issue";
      s.Description = "Creates a new issue";
    });
  }
}

public sealed class CreateIssueCommandValidator : Validator<CreateIssueCommand>
{
  public CreateIssueCommandValidator()
  {
    RuleFor(x => x.Title)
      .NotEmpty()
      .MinimumLength(IssueConstraints.TITLE_MIN_LENGTH)
      .MaximumLength(IssueConstraints.TITLE_MAX_LENGTH);

    RuleFor(x => x.Description)
      .NotEmpty()
      .MaximumLength(IssueConstraints.DESCRIPTION_MAX_LENGTH);

    RuleFor(x => x.Status)
      .IsInEnum();

    RuleFor(x => x.Priority)
      .IsInEnum();

    RuleForEach(x => x.AcceptanceCriteria)
      .ChildRules(acceptanceCriterion =>
      {
        acceptanceCriterion.RuleFor(x => x.Content)
          .NotEmpty()
          .MaximumLength(IssueAcceptanceCriterionConstraints.CONTENT_MAX_LENGTH);
      });
  }
}
