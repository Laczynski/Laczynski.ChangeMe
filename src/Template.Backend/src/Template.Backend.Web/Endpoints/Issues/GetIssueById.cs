using Template.Backend.UseCases.Issues;
using Template.Backend.UseCases.Issues.Dtos;

namespace Template.Backend.Web.Endpoints.Issues;

public class GetIssueById(IMediator _mediator) : BaseEndpoint<GetIssueByIdQuery, IssueDetailsDto>(_mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/issues/{id}");
    Summary(s =>
    {
      s.Summary = "Get issue by ID";
      s.Description = "Get an issue by ID";
    });
  }
}

public sealed class GetIssueByIdQueryValidator : Validator<GetIssueByIdQuery>
{
  public GetIssueByIdQueryValidator()
  {
    RuleFor(x => x.Id)
      .NotEmpty();
  }
}
