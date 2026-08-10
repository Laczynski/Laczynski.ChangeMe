using DataGrid.Abstractions;
using Template.Backend.UseCases.Issues;
using Template.Backend.UseCases.Issues.Dtos;

namespace Template.Backend.Web.Endpoints.Issues;

public class GetAllIssues(IMediator mediator) : BaseEndpoint<GetAllIssuesQuery, GridResult<IssueDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/issues");
    Summary(s =>
    {
      s.Summary = "Get all issues";
      s.Description = "Gets a paged issues list with grid query transport and sorting";
    });
  }
}
