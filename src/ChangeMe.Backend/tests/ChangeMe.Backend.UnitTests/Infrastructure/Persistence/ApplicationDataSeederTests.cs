using ChangeMe.Backend.Domain.Aggregates.Roles;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Infrastructure.Persistence;
using ChangeMe.Backend.UnitTests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace ChangeMe.Backend.UnitTests.Infrastructure.Persistence;

public sealed class ApplicationDataSeederTests
{
  [Fact]
  public async Task SeedAsync_OnEmptyDatabase_CreatesInitialAdministratorInSingleRun()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    const string email = "bootstrap@example.local";

    await using var context = UseCasesTestDb.Create(nameof(SeedAsync_OnEmptyDatabase_CreatesInitialAdministratorInSingleRun));

    var configuration = new ConfigurationBuilder()
      .AddInMemoryCollection(new Dictionary<string, string?>
      {
        [$"{InitialAdministratorOptions.SectionName}:Email"] = email,
        [$"{InitialAdministratorOptions.SectionName}:Password"] = "BootstrapPass123!",
        [$"{InitialAdministratorOptions.SectionName}:FirstName"] = "Bootstrap",
        [$"{InitialAdministratorOptions.SectionName}:LastName"] = "Admin",
      })
      .Build();

    await ApplicationDataSeeder.SeedAsync(
      context,
      configuration,
      new TestPasswordHasher(),
      NullLogger.Instance,
      cancellationToken);

    var administrator = await context.Users
      .Include(x => x.Roles)
      .SingleOrDefaultAsync(x => x.NormalizedEmail == User.NormalizeEmail(email), cancellationToken);

    Assert.NotNull(administrator);

    var administratorRoleId = await context.Roles
      .Where(r => r.Name == RoleConstraints.AdministratorRoleName)
      .Select(r => r.Id)
      .SingleAsync(cancellationToken);

    Assert.Contains(administrator.Roles, x => x.RoleId == administratorRoleId);
  }

  private sealed class TestPasswordHasher : IPasswordHasher
  {
    public string HashPassword(string password) => $"hash:{password}";
    public bool VerifyPassword(string passwordHash, string password) => passwordHash == $"hash:{password}";
  }
}
