using ChangeMe.Backend.Domain.Aggregates.Issue;
using ChangeMe.Backend.Domain.Aggregates.Issue.Entities;
using ChangeMe.Backend.Infrastructure.FileStorage;
using ChangeMe.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UnitTests.Infrastructure.FileStorage;

public sealed class AttachmentStorageCleanupJobTests
{
  [Fact]
  public async Task ExecuteAsync_ShouldRemoveOrphanedStoredFiles()
  {
    var rootPath = Path.Combine(Path.GetTempPath(), $"template-cleanup-{Guid.NewGuid():N}");
    Directory.CreateDirectory(rootPath);

    var options = Options.Create(new FileStorageOptions
    {
      RootPath = rootPath
    });

    await using var context = CreateInMemoryContext();

    var issueId = Guid.CreateVersion7();
    var attachment = IssueAttachment.Create(
      issueId,
      "active.txt",
      "text/plain",
      5).Value;
    attachment.CreatedAt = DateTime.UtcNow;
    attachment.CreatedBy = Guid.CreateVersion7();

    context.Attachments.Add(attachment);
    await context.SaveChangesAsync(TestContext.Current.CancellationToken);

    var storage = new LocalFileStorageService(options);
    await using (var activeStream = new MemoryStream("active"u8.ToArray()))
      await storage.SaveAsync(
        IssueConstraints.STORAGE_CONTAINER,
        issueId,
        attachment.StorageKey,
        activeStream,
        TestContext.Current.CancellationToken);

    var orphanOwnerId = Guid.CreateVersion7();
    var orphanKey = IssueAttachment.Create(orphanOwnerId, "orphan.txt", "text/plain", 1).Value.StorageKey;
    await using (var orphanStream = new MemoryStream("orphan"u8.ToArray()))
      await storage.SaveAsync(
        IssueConstraints.STORAGE_CONTAINER,
        orphanOwnerId,
        orphanKey,
        orphanStream,
        TestContext.Current.CancellationToken);

    var job = new AttachmentStorageCleanupJob(
      context,
      storage,
      NullLogger<AttachmentStorageCleanupJob>.Instance);

    await job.ExecuteAsync(TestContext.Current.CancellationToken);

    Assert.True(await context.Attachments.AnyAsync(a => a.Id == attachment.Id, TestContext.Current.CancellationToken));

    var storedKeys = await storage.ListStoredFileKeysAsync(TestContext.Current.CancellationToken);
    Assert.Single(storedKeys);
    Assert.Equal(attachment.StorageKey, storedKeys[0].StorageKey);

    Directory.Delete(rootPath, recursive: true);
  }

  private static ApplicationDbContext CreateInMemoryContext()
  {
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
      .UseInMemoryDatabase(Guid.NewGuid().ToString())
      .Options;

    return new ApplicationDbContext(options);
  }
}
