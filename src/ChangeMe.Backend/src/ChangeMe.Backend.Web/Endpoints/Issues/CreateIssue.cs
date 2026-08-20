using ChangeMe.Backend.Domain.Aggregates.Issue;
using ChangeMe.Backend.Domain.Aggregates.Issue.Entities;
using ChangeMe.Backend.UseCases.Issues;
using ChangeMe.Backend.UseCases.Issues.Dtos;

namespace ChangeMe.Backend.Web.Endpoints.Issues;

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
