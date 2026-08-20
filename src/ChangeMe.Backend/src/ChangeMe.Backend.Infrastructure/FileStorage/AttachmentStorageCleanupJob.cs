using ChangeMe.Backend.Infrastructure.Persistence;
using Hangfire;

namespace ChangeMe.Backend.Infrastructure.FileStorage;

public sealed class AttachmentStorageCleanupJob(
  ApplicationDbContext context,
  IFileStorageService fileStorageService,
  ILogger<AttachmentStorageCleanupJob> logger)
{
  public Task ExecuteAsync(IJobCancellationToken jobCancellationToken)
  {
    jobCancellationToken.ThrowIfCancellationRequested();
    return ExecuteAsync(jobCancellationToken.ShutdownToken);
  }

  public async Task ExecuteAsync(CancellationToken cancellationToken)
  {
    var knownKeys = await context.Attachments
      .AsNoTracking()
      .Select(a => new { a.StorageContainer, a.OwnerId, a.StorageKey })
      .ToListAsync(cancellationToken);

    var knownKeySet = knownKeys
      .Select(x => (x.StorageContainer, x.OwnerId, x.StorageKey))
      .ToHashSet();

    var storedKeys = await fileStorageService.ListStoredFileKeysAsync(cancellationToken);
    var orphanedFiles = storedKeys
      .Where(key => !knownKeySet.Contains((key.Container, key.OwnerId, key.StorageKey)))
      .ToList();

    foreach (var group in orphanedFiles.GroupBy(file => (file.Container, file.OwnerId)))
    {
      await fileStorageService.DeleteManyAsync(
        group.Key.Container,
        group.Key.OwnerId,
        group.Select(file => file.StorageKey),
        cancellationToken);
    }

    if (orphanedFiles.Count > 0)
    {
      logger.LogInformation(
        "Attachment cleanup removed {DeletedCount} orphaned stored files",
        orphanedFiles.Count);
    }
  }
}
