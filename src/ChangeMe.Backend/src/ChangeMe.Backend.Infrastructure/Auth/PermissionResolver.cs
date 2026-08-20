using ChangeMe.Backend.Infrastructure.Persistence;

namespace ChangeMe.Backend.Infrastructure.Auth;

public static class PermissionResolver
{
  public static async Task<IReadOnlyList<string>> GetEffectivePermissionsAsync(
    ApplicationDbContext context,
    Guid userId,
    CancellationToken cancellationToken)
  {
    return await context.Users
      .AsNoTracking()
      .Where(x => x.Id == userId)
      .SelectMany(x => x.Roles)
      .SelectMany(x => x.Role.Permissions.Select(p => p.PermissionCode))
      .Distinct()
      .OrderBy(x => x)
      .ToListAsync(cancellationToken);
  }
}
