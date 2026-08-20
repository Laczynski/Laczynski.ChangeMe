using ChangeMe.Backend.DataGenerator.Options;
using ChangeMe.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.DataGenerator.Services;

internal sealed class DemoDataCleaner(ApplicationDbContext dbContext, IOptions<DataGeneratorOptions> options)
{
  public async Task CleanAsync(CancellationToken cancellationToken)
  {
    var demoUserIds = await GetDemoUserIdsAsync(cancellationToken);
    if (demoUserIds.Count == 0)
      return;

    var demoIssueIds = await dbContext.Issues
      .Where(i => demoUserIds.Contains(i.CreatedBy) || (i.AssignedToUserId != null && demoUserIds.Contains(i.AssignedToUserId.Value)))
      .Select(i => i.Id)
      .ToListAsync(cancellationToken);

    await dbContext.Notifications
      .Where(n => demoUserIds.Contains(n.RecipientUserId) || demoIssueIds.Contains(n.IssueId))
      .ExecuteDeleteAsync(cancellationToken);

    if (demoIssueIds.Count > 0)
    {
      await dbContext.Issues
        .Where(i => demoIssueIds.Contains(i.Id))
        .ExecuteDeleteAsync(cancellationToken);
    }

    await dbContext.UserSessions
      .Where(s => demoUserIds.Contains(s.UserId))
      .ExecuteDeleteAsync(cancellationToken);

    await dbContext.Users
      .Where(u => demoUserIds.Contains(u.Id))
      .ExecuteDeleteAsync(cancellationToken);

    await dbContext.SaveChangesAsync(cancellationToken);
  }

  private async Task<List<Guid>> GetDemoUserIdsAsync(CancellationToken cancellationToken)
  {
    var emailSuffix = $"@{options.Value.EmailDomain.Trim().ToUpperInvariant()}";

    return await dbContext.Users
      .Where(u => u.NormalizedEmail.EndsWith(emailSuffix))
      .Select(u => u.Id)
      .ToListAsync(cancellationToken);
  }
}
