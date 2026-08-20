using ChangeMe.Backend.Domain.Aggregates.Issue;
using ChangeMe.Backend.Domain.Aggregates.Issue.Entities;
using ChangeMe.Backend.UseCases.Issues;
using ChangeMe.Backend.UseCases.Issues.Dtos;

namespace ChangeMe.Backend.Web.Endpoints.Issues;

public class UpdateIssue(IMediator mediator) : BaseEndpoint<UpdateIssueCommand, IssueDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Put("/issues/{id}");
    Summary(s =>
    {
      s.Summary = "Update issue";
      s.Description = "Updates an issue by ID";
    });
  }
}

public sealed class UpdateIssueCommandValidator : Validator<UpdateIssueCommand>
{
  public UpdateIssueCommandValidator()
  {
    RuleFor(x => x.Id)
      .NotEmpty();

    RuleFor(x => x.Version)
      .GreaterThanOrEqualTo(0);

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
