using Template.Backend.Domain.Aggregates.Issue;
using Template.Backend.Infrastructure.FileStorage;
using Template.Backend.UseCases.Issues.Dtos;
using Template.Backend.UseCases.Issues.Services;
using Template.Backend.UseCases.Issues.Utils;

namespace Template.Backend.UseCases.Issues.Attachments;

public record UploadIssueAttachmentCommand(
  Guid IssueId,
  string OriginalFileName,
  string? DeclaredContentType,
  byte[] Content) : ICommand<IssueAttachmentDto>;

public class UploadIssueAttachmentHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  IFileContentValidator fileContentValidator,
  IFileStorageService fileStorageService,
  IssueNotificationService issueNotificationService) : ICommandHandler<UploadIssueAttachmentCommand, IssueAttachmentDto>
{
  public async ValueTask<Result<IssueAttachmentDto>> Handle(
    UploadIssueAttachmentCommand command,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid actorUserId)
      return Result<IssueAttachmentDto>.Unauthorized();

    var validationResult = fileContentValidator.Validate(
      command.OriginalFileName,
      command.DeclaredContentType,
      command.Content,
      command.Content.LongLength,
      IssueConstraints.ATTACHMENT_MAX_FILE_SIZE_BYTES,
      IssueConstraints.ATTACHMENT_ALLOWED_EXTENSIONS);

    if (!validationResult.IsSuccess)
      return validationResult.Map();

    var issue = await context.Issues
      .Include(i => i.Attachments)
      .Include(i => i.HistoryEntries)
      .Include(i => i.Watchers)
      .FirstOrDefaultAsync(i => i.Id == command.IssueId, cancellationToken);

    if (issue is null)
      return Result<IssueAttachmentDto>.NotFound();

    var historyCountBeforeAdd = issue.HistoryEntries.Count;

    var addResult = issue.AddAttachment(
      validationResult.Value.SanitizedFileName,
      validationResult.Value.ContentType,
      command.Content.LongLength,
      actorUserId);

    if (!addResult.IsSuccess)
      return addResult.Map();

    var attachment = addResult.Value;
    context.Attachments.Add(attachment);

    var newHistoryEntries = issue.HistoryEntries
      .Skip(historyCountBeforeAdd)
      .ToList();

    await using var contentStream = new MemoryStream(command.Content, writable: false);
    var storageResult = await fileStorageService.SaveAsync(
      attachment.StorageContainer,
      attachment.OwnerId,
      attachment.StorageKey,
      contentStream,
      cancellationToken);

    if (!storageResult.IsSuccess)
    {
      await fileStorageService.DeleteAsync(
        attachment.StorageContainer,
        attachment.OwnerId,
        attachment.StorageKey,
        cancellationToken);
      return storageResult.Map();
    }

    await context.SaveChangesAsync(cancellationToken);

    foreach (var historyEntryId in newHistoryEntries
               .Where(h => IssuesUtils.IsNotificationEligible(h.EventType))
               .Select(h => h.Id))
      await issueNotificationService.NotifyIssueActivityAsync(issue.Id, historyEntryId, actorUserId, cancellationToken);

    var userLookup = await IssuesUtils.GetUserDisplayNameLookupAsync(
      context,
      [actorUserId],
      cancellationToken);

    return Result.Created(new IssueAttachmentDto
    {
      Id = attachment.Id,
      OriginalFileName = attachment.OriginalFileName,
      ContentType = attachment.ContentType,
      SizeBytes = attachment.SizeBytes,
      UploadedByUserId = actorUserId,
      UploadedByName = userLookup.GetValueOrDefault(actorUserId),
      CreatedAt = attachment.CreatedAt,
      CanDelete = true
    }, $"/issues/{issue.Id}/attachments/{attachment.Id}");
  }
}
