using Microsoft.EntityFrameworkCore;
using Template.Backend.Domain.Aggregates.Roles;
using Template.Backend.Infrastructure.Persistence;

namespace Template.Backend.UnitTests.Support;

internal static class UseCasesTestDb
{
  public static ApplicationDbContext Create(string databaseName)
  {
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
      .UseInMemoryDatabase(databaseName)
      .Options;

    return new ApplicationDbContext(options);
  }

  public static async Task SeedSystemRolesAsync(ApplicationDbContext context)
  {
    if (!await context.Roles.AnyAsync(x => x.Name == RoleConstraints.UserRoleName))
      await context.Roles.AddAsync(Role.CreateDefaultUserRole());

    if (!await context.Roles.AnyAsync(x => x.Name == RoleConstraints.AdministratorRoleName))
      await context.Roles.AddAsync(Role.CreateAdministrator());

    await context.SaveChangesAsync();
  }
}
