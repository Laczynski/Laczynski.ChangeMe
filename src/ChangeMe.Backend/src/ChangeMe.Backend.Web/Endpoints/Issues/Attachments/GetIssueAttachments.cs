using ChangeMe.Backend.UseCases.Issues.Attachments;
using ChangeMe.Backend.UseCases.Issues.Dtos;
using DataGrid.Abstractions;

namespace ChangeMe.Backend.Web.Endpoints.Issues.Attachments;

public class GetIssueAttachments(IMediator mediator)
  : BaseEndpoint<GetIssueAttachmentsQuery, GridResult<IssueAttachmentDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/issues/{IssueId}/attachments");
    Summary(s => s.Summary = "Get issue attachments");
  }
}
