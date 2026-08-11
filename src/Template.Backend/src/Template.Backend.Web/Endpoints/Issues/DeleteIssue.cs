using Template.Backend.UseCases.Issues;

namespace Template.Backend.Web.Endpoints.Issues;

public class DeleteIssue(IMediator _mediator) : BaseEndpoint<DeleteIssueCommand, Guid>(_mediator)
{
  protected override void ConfigureEndpoint()
  {
    Delete("/issues/{id}");
    Summary(s =>
    {
      s.Summary = "Delete issue";
      s.Description = "Delete a issue by ID";
    });
  }
}

public sealed class DeleteIssueCommandValidator : Validator<DeleteIssueCommand>
{
  public DeleteIssueCommandValidator()
  {
    RuleFor(x => x.Id)
      .NotEmpty();
  }
}
