using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Template.Backend.DataGenerator.Options;
using Template.Backend.Infrastructure.Persistence;

namespace Template.Backend.DataGenerator.Services;

internal sealed class DemoDataExistsChecker(ApplicationDbContext dbContext, IOptions<DataGeneratorOptions> options)
{
  public async Task<bool> HasDemoDataAsync(CancellationToken cancellationToken)
  {
    var emailSuffix = $"@{options.Value.EmailDomain.Trim().ToUpperInvariant()}";

    return await dbContext.Users
      .AnyAsync(u => u.NormalizedEmail.EndsWith(emailSuffix), cancellationToken);
  }
}
