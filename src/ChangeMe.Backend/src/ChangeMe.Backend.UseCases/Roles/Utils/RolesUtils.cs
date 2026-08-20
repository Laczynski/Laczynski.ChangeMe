using ChangeMe.Backend.Domain.Aggregates.Roles;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Roles.Dtos;

namespace ChangeMe.Backend.UseCases.Roles.Utils;

public static class RolesUtils
{
  public const string PermissionDeniedMessage = "You do not have permission to perform this action.";
  public const string DuplicateNameMessage = "A role with this name already exists.";
  public const string SystemRoleCannotBeModifiedMessage = "System roles cannot be modified.";
  public const string AtLeastOnePermissionRequiredMessage = "At least one permission is required.";
  public const string RoleAssignedToUsersMessage =
    "Role is assigned to one or more users. Remove all user assignments before deleting this role.";
  public const string UserMustHaveRoleMessage =
    "Each user must have at least one role. Assign another role before removing this one.";

  public static IReadOnlyList<PermissionCatalogItemDto> GetPermissionCatalog() =>
    PermissionCatalog.All
      .Select(x => new PermissionCatalogItemDto(x.Code, x.Label, x.Description, x.Group))
      .ToList();

  public static IReadOnlyList<RolePermissionItemDto> MapRolePermissions(IEnumerable<string> permissionCodes) =>
    permissionCodes
      .Select(code =>
      {
        var definition = PermissionCatalog.Find(code);
        return new RolePermissionItemDto(
          code,
          definition?.Label ?? code,
          definition?.Description ?? string.Empty,
          definition?.Group ?? "Other");
      })
      .OrderBy(x => x.Group, StringComparer.Ordinal)
      .ThenBy(x => x.Label, StringComparer.Ordinal)
      .ToList();

  public static Result ValidatePermissionCodes(IReadOnlyList<string> permissionCodes)
  {
    if (permissionCodes.Count == 0)
      return Result.Error(AtLeastOnePermissionRequiredMessage);

    var validCodes = PermissionCatalog.All.Select(x => x.Code).ToHashSet(StringComparer.Ordinal);
    if (permissionCodes.Any(code => !validCodes.Contains(code)))
      return Result.NotFound();

    return Result.Success();
  }

  public static async Task<bool> IsNameTakenAsync(
    ApplicationDbContext context,
    string name,
    Guid? excludeRoleId,
    CancellationToken cancellationToken)
  {
    var normalizedName = Role.NormalizeName(name);
    return await context.Roles
      .AsNoTracking()
      .AnyAsync(
        x => x.Name.ToUpper() == normalizedName
          && (!excludeRoleId.HasValue || x.Id != excludeRoleId.Value),
        cancellationToken);
  }

  public static async Task<(string FirstName, string LastName, string Email)> GetUserIdentityAsync(
    ApplicationDbContext context,
    Guid userId,
    CancellationToken cancellationToken)
  {
    return await context.Users
      .AsNoTracking()
      .Where(x => x.Id == userId)
      .Select(x => new ValueTuple<string, string, string>(x.FirstName, x.LastName, x.Email))
      .SingleAsync(cancellationToken);
  }
}
