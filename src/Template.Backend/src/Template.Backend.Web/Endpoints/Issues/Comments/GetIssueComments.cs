using DataGrid.Abstractions;
using Template.Backend.UseCases.Issues.Comments;
using Template.Backend.UseCases.Issues.Dtos;

namespace Template.Backend.Web.Endpoints.Issues.Comments;

public class GetIssueComments(IMediator mediator)
  : BaseEndpoint<GetIssueCommentsQuery, GridResult<IssueCommentDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/issues/{IssueId}/comments");
    Summary(s => s.Summary = "Get issue comments");
  }
}
