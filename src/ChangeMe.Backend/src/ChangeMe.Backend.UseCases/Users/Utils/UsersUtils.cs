using ChangeMe.Backend.Domain.Aggregates.Roles;
using ChangeMe.Backend.Domain.Aggregates.Sessions;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Users.Dtos;

namespace ChangeMe.Backend.UseCases.Users.Utils;

public static class UsersUtils
{
  public const string DuplicateEmailMessage = "A user with this email already exists.";
  public const string CannotRemoveOwnAdministratorMessage = "You cannot remove your own administrator access.";
  public const string CannotDeactivateOwnAccountMessage = "You cannot deactivate your own account.";
  public const string CannotDeactivateLastAdministratorMessage =
    "You cannot deactivate the last active administrator.";
  public const string CannotChangeOwnRolesMessage = "You cannot change your own roles.";
  public const string AtLeastOneRoleRequiredMessage = "At least one role is required.";
  public const string PermissionDeniedMessage = "You do not have permission to perform this action.";

  public static async Task<DateTime?> GetLastSignInAtAsync(
    ApplicationDbContext context,
    Guid userId,
    CancellationToken cancellationToken)
  {
    return await context.UserSessions
      .AsNoTracking()
      .Where(x => x.UserId == userId)
      .OrderByDescending(x => x.SignedInAt)
      .Select(x => (DateTime?)x.SignedInAt)
      .FirstOrDefaultAsync(cancellationToken);
  }

  public static async Task<IReadOnlyList<EffectivePermissionDto>> GetEffectivePermissionsForUserAsync(
    ApplicationDbContext context,
    Guid userId,
    CancellationToken cancellationToken)
  {
    var grants = await context.Users
      .AsNoTracking()
      .Where(x => x.Id == userId)
      .SelectMany(x => x.Roles)
      .SelectMany(x => x.Role.Permissions.Select(p => new { p.PermissionCode, x.Role.Name }))
      .ToListAsync(cancellationToken);

    return PermissionCatalog
      .BuildEffectivePermissions(grants.Select(x => (x.PermissionCode, x.Name)))
      .Select(x => new EffectivePermissionDto(
        x.Code,
        x.Label,
        x.Description,
        x.Group,
        x.FromRoleNames))
      .ToList();
  }

  public static async Task<IReadOnlyList<EffectivePermissionDto>> GetEffectivePermissionsForRolesAsync(
    ApplicationDbContext context,
    IReadOnlyList<Guid> roleIds,
    CancellationToken cancellationToken)
  {
    if (roleIds.Count == 0)
      return [];

    var grants = await context.Roles
      .AsNoTracking()
      .Where(x => roleIds.Contains(x.Id))
      .SelectMany(x => x.Permissions.Select(p => new { p.PermissionCode, x.Name }))
      .ToListAsync(cancellationToken);

    return PermissionCatalog
      .BuildEffectivePermissions(grants.Select(x => (x.PermissionCode, x.Name)))
      .Select(x => new EffectivePermissionDto(
        x.Code,
        x.Label,
        x.Description,
        x.Group,
        x.FromRoleNames))
      .ToList();
  }

  public static async Task<Result> ValidateCanDeactivateUserAsync(
    ApplicationDbContext context,
    Guid userId,
    CancellationToken cancellationToken)
  {
    var administratorRoleId = await context.Roles
      .AsNoTracking()
      .Where(x => x.Name == RoleConstraints.AdministratorRoleName)
      .Select(x => x.Id)
      .FirstOrDefaultAsync(cancellationToken);

    if (administratorRoleId == Guid.Empty)
      return Result.Success();

    var targetIsActiveAdministrator = await context.Users
      .AsNoTracking()
      .Where(x => x.Id == userId && !x.Deactivated)
      .AnyAsync(
        x => x.Roles.Any(ur => ur.RoleId == administratorRoleId),
        cancellationToken);

    if (!targetIsActiveAdministrator)
      return Result.Success();

    var hasAnotherActiveAdministrator = await context.Users
      .AsNoTracking()
      .Where(x => x.Id != userId && !x.Deactivated)
      .AnyAsync(
        x => x.Roles.Any(ur =>
          ur.RoleId == administratorRoleId &&
          ur.Role.Permissions.Any(p => p.PermissionCode == PermissionCodes.UsersDeactivate)),
        cancellationToken);

    if (!hasAnotherActiveAdministrator)
      return Result.Error(CannotDeactivateLastAdministratorMessage);

    return Result.Success();
  }

  public static async Task RevokeAllActiveSessionsAsync(
    ApplicationDbContext context,
    Guid userId,
    CancellationToken cancellationToken)
  {
    var utcNow = DateTime.UtcNow;
    var sessions = await context.UserSessions
      .Where(x => x.UserId == userId && x.RevokedAt == null)
      .ToListAsync(cancellationToken);

    foreach (var session in sessions)
      session.Revoke(utcNow);
  }

  public static IQueryable<UserSession> WhereActiveSessions(
    this IQueryable<UserSession> sessions,
    Guid userId,
    DateTime utcNow,
    int sessionLifetimeDays) =>
    sessions.Where(x =>
      x.UserId == userId
      && x.RevokedAt == null
      && x.SignedInAt.AddDays(sessionLifetimeDays) > utcNow);

  public static Task<bool> IsProfileEmailTakenAsync(
    ApplicationDbContext context,
    string normalizedEmail,
    Guid? excludeUserId,
    CancellationToken cancellationToken) =>
    context.Users.AnyAsync(
      x => (excludeUserId == null || x.Id != excludeUserId)
           && x.NormalizedEmail == normalizedEmail,
      cancellationToken);
}
