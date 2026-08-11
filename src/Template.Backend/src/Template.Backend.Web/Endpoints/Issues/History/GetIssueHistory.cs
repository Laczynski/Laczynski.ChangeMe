using DataGrid.Abstractions;
using Template.Backend.UseCases.Issues.Dtos;
using Template.Backend.UseCases.Issues.History;

namespace Template.Backend.Web.Endpoints.Issues.History;

public class GetIssueHistory(IMediator mediator)
  : BaseEndpoint<GetIssueHistoryQuery, GridResult<IssueHistoryEntryDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/issues/{IssueId}/history");
    Summary(s => s.Summary = "Get issue history");
  }
}
