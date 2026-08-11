using Template.Backend.Infrastructure.FileStorage;
using Template.Backend.UseCases.Issues.Dtos;

namespace Template.Backend.UseCases.Issues.Attachments;

public record GetIssueAttachmentContentQuery(Guid IssueId, Guid AttachmentId) : IQuery<IssueAttachmentContentDto>;

public class GetIssueAttachmentContentHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  IFileStorageService fileStorageService) : IQueryHandler<GetIssueAttachmentContentQuery, IssueAttachmentContentDto>
{
  public async ValueTask<Result<IssueAttachmentContentDto>> Handle(
    GetIssueAttachmentContentQuery query,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is null)
      return Result<IssueAttachmentContentDto>.Unauthorized();

    var attachment = await context.IssueAttachments
      .AsNoTracking()
      .FirstOrDefaultAsync(
        a => a.OwnerId == query.IssueId
          && a.Id == query.AttachmentId,
        cancellationToken);

    if (attachment is null)
      return Result<IssueAttachmentContentDto>.NotFound();

    var streamResult = await fileStorageService.OpenReadStreamAsync(
      attachment.StorageContainer,
      attachment.OwnerId,
      attachment.StorageKey,
      cancellationToken);

    if (!streamResult.IsSuccess)
      return streamResult.Map();

    return Result.Success(new IssueAttachmentContentDto
    {
      Content = streamResult.Value,
      OriginalFileName = attachment.OriginalFileName,
      ContentType = attachment.ContentType
    });
  }
}
