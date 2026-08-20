using ChangeMe.Backend.Domain.Aggregates.Roles;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Authorization;

namespace ChangeMe.Backend.Infrastructure.Persistence;

public static class ApplicationDataSeeder
{
  public static async Task SeedAsync(
    ApplicationDbContext context,
    IConfiguration configuration,
    IPasswordHasher passwordHasher,
    ILogger logger,
    CancellationToken cancellationToken)
  {
    var administratorRole = await EnsureSystemRolesAsync(context, cancellationToken);
    await EnsureInitialAdministratorAsync(
      context,
      configuration,
      passwordHasher,
      logger,
      administratorRole,
      cancellationToken);
    await context.SaveChangesAsync(cancellationToken);
  }

  private static async Task<Role> EnsureSystemRolesAsync(ApplicationDbContext context, CancellationToken cancellationToken)
  {
    var administratorRole = await context.Roles
      .Include(x => x.Permissions)
      .FirstOrDefaultAsync(x => x.Name == RoleConstraints.AdministratorRoleName, cancellationToken);

    if (administratorRole is null)
    {
      administratorRole = Role.CreateAdministrator();
      await context.Roles.AddAsync(administratorRole, cancellationToken);
    }
    else
    {
      foreach (var permission in PermissionCodes.All)
        administratorRole.AddPermissionIfMissing(permission);
    }

    var userRole = await context.Roles
      .Include(x => x.Permissions)
      .FirstOrDefaultAsync(x => x.Name == RoleConstraints.UserRoleName, cancellationToken);

    if (userRole is null)
    {
      await context.Roles.AddAsync(Role.CreateDefaultUserRole(), cancellationToken);
    }
    else
    {
      foreach (var permission in PermissionCodes.DefaultUserRole)
        userRole.AddPermissionIfMissing(permission);
    }

    return administratorRole;
  }

  private static async Task EnsureInitialAdministratorAsync(
    ApplicationDbContext context,
    IConfiguration configuration,
    IPasswordHasher passwordHasher,
    ILogger logger,
    Role administratorRole,
    CancellationToken cancellationToken)
  {
    var options = configuration.GetSection(InitialAdministratorOptions.SectionName).Get<InitialAdministratorOptions>();
    if (options is null ||
        string.IsNullOrWhiteSpace(options.Email) ||
        string.IsNullOrWhiteSpace(options.Password) ||
        string.IsNullOrWhiteSpace(options.FirstName) ||
        string.IsNullOrWhiteSpace(options.LastName))
    {
      return;
    }

    var normalizedEmail = User.NormalizeEmail(options.Email);
    var existingUser = await context.Users
      .FirstOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);

    if (existingUser is not null)
      return;

    var passwordHash = passwordHasher.HashPassword(options.Password);
    var createUserResult = User.Create(
      options.FirstName,
      options.LastName,
      options.Email,
      passwordHash);
    if (!createUserResult.IsSuccess)
      return;

    var administrator = createUserResult.Value;
    administrator.AssignRole(administratorRole.Id);
    await context.Users.AddAsync(administrator, cancellationToken);

    logger.LogInformation("Initial administrator account created for {Email}", options.Email);
  }
}
