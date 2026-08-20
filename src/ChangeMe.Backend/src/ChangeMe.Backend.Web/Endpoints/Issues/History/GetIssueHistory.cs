using ChangeMe.Backend.UseCases.Issues.Dtos;
using ChangeMe.Backend.UseCases.Issues.History;
using DataGrid.Abstractions;

namespace ChangeMe.Backend.Web.Endpoints.Issues.History;

public class GetIssueHistory(IMediator mediator)
  : BaseEndpoint<GetIssueHistoryQuery, GridResult<IssueHistoryEntryDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/issues/{IssueId}/history");
    Summary(s => s.Summary = "Get issue history");
  }
}
