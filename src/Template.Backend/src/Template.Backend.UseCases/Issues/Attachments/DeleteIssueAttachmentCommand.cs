using Template.Backend.Infrastructure.FileStorage;
using Template.Backend.UseCases.Issues.Services;
using Template.Backend.UseCases.Issues.Utils;

namespace Template.Backend.UseCases.Issues.Attachments;

public record DeleteIssueAttachmentCommand(Guid IssueId, Guid AttachmentId) : ICommand<Guid>;

public class DeleteIssueAttachmentHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  IFileStorageService fileStorageService,
  IssueNotificationService issueNotificationService) : ICommandHandler<DeleteIssueAttachmentCommand, Guid>
{
  public async ValueTask<Result<Guid>> Handle(
    DeleteIssueAttachmentCommand command,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid actorUserId)
      return Result<Guid>.Unauthorized();

    var issue = await context.Issues
      .Include(i => i.Attachments)
      .Include(i => i.HistoryEntries)
      .Include(i => i.Watchers)
      .FirstOrDefaultAsync(i => i.Id == command.IssueId, cancellationToken);

    if (issue is null)
      return Result<Guid>.NotFound();

    var attachment = issue.Attachments.FirstOrDefault(a => a.Id == command.AttachmentId);
    if (attachment is null)
      return Result<Guid>.NotFound();

    if (attachment.CreatedBy != actorUserId)
      return Result<Guid>.Forbidden();

    var storageContainer = attachment.StorageContainer;
    var storageOwnerId = attachment.OwnerId;
    var storageKey = attachment.StorageKey;

    var historyCountBeforeRemove = issue.HistoryEntries.Count;

    var removeResult = issue.RemoveAttachment(command.AttachmentId, actorUserId);
    if (!removeResult.IsSuccess)
      return removeResult.Map();

    var newHistoryEntries = issue.HistoryEntries
      .Skip(historyCountBeforeRemove)
      .ToList();

    context.IssueAttachments.Remove(removeResult.Value);
    if (newHistoryEntries.Count > 0)
      await context.IssueHistoryEntries.AddRangeAsync(newHistoryEntries, cancellationToken);

    await context.SaveChangesAsync(cancellationToken);

    await fileStorageService.DeleteAsync(
      storageContainer,
      storageOwnerId,
      storageKey,
      cancellationToken);

    foreach (var historyEntryId in newHistoryEntries
               .Where(h => IssuesUtils.IsNotificationEligible(h.EventType))
               .Select(h => h.Id))
      await issueNotificationService.NotifyIssueActivityAsync(issue.Id, historyEntryId, actorUserId, cancellationToken);

    return Result.Success(removeResult.Value.Id);
  }
}
