using DataGrid.Abstractions;
using Template.Backend.UseCases.Issues.Attachments;
using Template.Backend.UseCases.Issues.Dtos;

namespace Template.Backend.Web.Endpoints.Issues.Attachments;

public class GetIssueAttachments(IMediator mediator)
  : BaseEndpoint<GetIssueAttachmentsQuery, GridResult<IssueAttachmentDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/issues/{IssueId}/attachments");
    Summary(s => s.Summary = "Get issue attachments");
  }
}
