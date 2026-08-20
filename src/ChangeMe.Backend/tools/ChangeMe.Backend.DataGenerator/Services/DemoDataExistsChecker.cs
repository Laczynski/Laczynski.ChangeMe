using ChangeMe.Backend.DataGenerator.Options;
using ChangeMe.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.DataGenerator.Services;

internal sealed class DemoDataExistsChecker(ApplicationDbContext dbContext, IOptions<DataGeneratorOptions> options)
{
  public async Task<bool> HasDemoDataAsync(CancellationToken cancellationToken)
  {
    var emailSuffix = $"@{options.Value.EmailDomain.Trim().ToUpperInvariant()}";

    return await dbContext.Users
      .AnyAsync(u => u.NormalizedEmail.EndsWith(emailSuffix), cancellationToken);
  }
}
