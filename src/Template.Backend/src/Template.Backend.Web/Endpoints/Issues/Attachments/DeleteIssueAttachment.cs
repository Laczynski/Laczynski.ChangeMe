using Template.Backend.UseCases.Issues.Attachments;

namespace Template.Backend.Web.Endpoints.Issues.Attachments;

public class DeleteIssueAttachment(IMediator mediator) : BaseEndpoint<DeleteIssueAttachmentCommand, Guid>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Delete("/issues/{IssueId}/attachments/{AttachmentId}");
    Summary(s => s.Summary = "Delete issue attachment");
  }
}

public sealed class DeleteIssueAttachmentCommandValidator : Validator<DeleteIssueAttachmentCommand>
{
  public DeleteIssueAttachmentCommandValidator()
  {
    RuleFor(x => x.IssueId)
      .NotEmpty();

    RuleFor(x => x.AttachmentId)
      .NotEmpty();
  }
}
