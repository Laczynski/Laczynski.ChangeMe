using ChangeMe.Backend.Infrastructure.FileStorage;

namespace ChangeMe.Backend.UseCases.Issues;

public record DeleteIssueCommand(
    Guid Id) : ICommand<Guid>;

public class DeleteIssueHandler(
  ApplicationDbContext context,
  IFileStorageService fileStorageService) : ICommandHandler<DeleteIssueCommand, Guid>
{
  public async ValueTask<Result<Guid>> Handle(DeleteIssueCommand command, CancellationToken cancellationToken)
  {
    var issue = await context.Issues
      .Include(i => i.Attachments)
      .FirstOrDefaultAsync(i => i.Id == command.Id, cancellationToken);

    if (issue is null)
      return Result.NotFound();

    var storageKeysByContainer = issue.Attachments
      .GroupBy(a => a.StorageContainer)
      .ToList();

    context.Issues.Remove(issue);
    await context.SaveChangesAsync(cancellationToken);

    foreach (var group in storageKeysByContainer)
    {
      var storageKeys = group.Select(a => a.StorageKey).ToList();
      if (storageKeys.Count > 0)
        await fileStorageService.DeleteManyAsync(group.Key, issue.Id, storageKeys, cancellationToken);
    }

    return Result.Success(issue.Id);
  }
}
