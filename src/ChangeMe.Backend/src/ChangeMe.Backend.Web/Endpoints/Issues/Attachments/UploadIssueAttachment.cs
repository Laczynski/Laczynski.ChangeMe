using ChangeMe.Backend.Domain.Aggregates.Issue;
using ChangeMe.Backend.UseCases.Issues.Attachments;
using ChangeMe.Backend.UseCases.Issues.Dtos;
using ChangeMe.Backend.Web.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace ChangeMe.Backend.Web.Endpoints.Issues.Attachments;

public sealed class UploadIssueAttachmentRequest
{
  public Guid IssueId { get; set; }
  public IFormFile File { get; set; } = default!;
}

public class UploadIssueAttachment(IMediator mediator) : Endpoint<UploadIssueAttachmentRequest, Result<IssueAttachmentDto>>
{
  public override void Configure()
  {
    Post("/issues/{IssueId}/attachments");
    Version(ApiVersionConfig.CurrentVersion);
    AllowFileUploads();
    AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    Summary(s => s.Summary = "Upload issue attachment");
  }

  public override async Task HandleAsync(UploadIssueAttachmentRequest req, CancellationToken ct)
  {
    req.IssueId = Route<Guid>("IssueId");
    var file = req.File ?? Form.Files.GetFile("File");
    Result<IssueAttachmentDto> response;

    if (file is null || file.Length == 0)
    {
      response = Result<IssueAttachmentDto>.Invalid(new ValidationError("File", "cannot be empty"));
    }
    else if (file.Length > IssueConstraints.ATTACHMENT_MAX_FILE_SIZE_BYTES)
    {
      response = Result<IssueAttachmentDto>.Invalid(
        new ValidationError("File", $"cannot exceed {IssueConstraints.ATTACHMENT_MAX_FILE_SIZE_BYTES} bytes"));
    }
    else
    {
      await using var stream = file.OpenReadStream();
      using var memoryStream = new MemoryStream();
      await stream.CopyToAsync(memoryStream, ct);

      response = await mediator.Send(
        new UploadIssueAttachmentCommand(
          req.IssueId,
          file.FileName,
          file.ContentType,
          memoryStream.ToArray()),
        ct) ?? Result<IssueAttachmentDto>.Error("Unknown error");
    }

    await HttpContext.SendResultAsync(response, ct);
  }
}
