using ChangeMe.Backend.UseCases.Issues.Comments;
using ChangeMe.Backend.UseCases.Issues.Dtos;
using DataGrid.Abstractions;

namespace ChangeMe.Backend.Web.Endpoints.Issues.Comments;

public class GetIssueComments(IMediator mediator)
  : BaseEndpoint<GetIssueCommentsQuery, GridResult<IssueCommentDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/issues/{IssueId}/comments");
    Summary(s => s.Summary = "Get issue comments");
  }
}
