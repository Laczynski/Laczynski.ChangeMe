using System.Net.Mime;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Template.Backend.UseCases.Issues.Attachments;
using Template.Backend.Web.Configurations;

namespace Template.Backend.Web.Endpoints.Issues.Attachments;

public class DownloadIssueAttachment(IMediator mediator) : EndpointWithoutRequest
{
  public override void Configure()
  {
    Get("/issues/{IssueId}/attachments/{AttachmentId}/content");
    Version(ApiVersionConfig.CurrentVersion);
    AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    Summary(s => s.Summary = "Download issue attachment content");
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var result = await mediator.Send(
      new GetIssueAttachmentContentQuery(
        Route<Guid>("IssueId"),
        Route<Guid>("AttachmentId")),
      ct);

    if (!result.IsSuccess)
    {
      await HttpContext.SendResultAsync(result, ct);
      return;
    }

    var content = result.Value;
    var response = HttpContext.Response;
    response.Headers.XContentTypeOptions = "nosniff";
    response.ContentType = content.ContentType;
    response.Headers.ContentDisposition = new ContentDisposition
    {
      FileName = content.OriginalFileName,
      DispositionType = DispositionTypeNames.Attachment
    }.ToString();

    await using (content.Content)
      await content.Content.CopyToAsync(response.Body, ct);
  }
}
