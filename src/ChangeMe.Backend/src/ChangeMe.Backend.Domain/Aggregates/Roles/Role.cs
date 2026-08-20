using ChangeMe.Backend.Domain.Aggregates.Roles.Entities;
using ChangeMe.Backend.Domain.Authorization;

namespace ChangeMe.Backend.Domain.Aggregates.Roles;

public class Role : Entity, IAggregateRoot
{
  private readonly List<RolePermission> permissions = [];

  private Role() { }

  public string Name { get; private set; } = string.Empty;
  public string? Description { get; private set; }
  public bool IsSystem { get; private set; }

  public IReadOnlyCollection<RolePermission> Permissions => permissions;

  public static Role CreateAdministrator()
  {
    var role = new Role
    {
      Name = RoleConstraints.AdministratorRoleName,
      Description = RoleConstraints.AdministratorDescription,
      IsSystem = true,
      CreatedBy = Guid.Empty,
      UpdatedBy = Guid.Empty,
    };

    role.SetPermissions(PermissionCodes.All);
    return role;
  }

  public static Role CreateDefaultUserRole()
  {
    var role = new Role
    {
      Name = RoleConstraints.UserRoleName,
      Description = RoleConstraints.UserDescription,
      IsSystem = true,
      CreatedBy = Guid.Empty,
      UpdatedBy = Guid.Empty,
    };

    role.SetPermissions(PermissionCodes.DefaultUserRole);
    return role;
  }

  public static Result<Role> Create(string name, string? description)
  {
    var validationErrors = new List<ValidationError>();
    ValidateName(name, validationErrors);

    if (description is not null && description.Trim().Length > RoleConstraints.DESCRIPTION_MAX_LENGTH)
      validationErrors.Add(new ValidationError(nameof(Description), $"cannot be longer than {RoleConstraints.DESCRIPTION_MAX_LENGTH} characters"));

    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    var role = new Role
    {
      Name = name.Trim(),
      Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
      IsSystem = false
    };

    return Result.Success(role);
  }

  public Result UpdateProfile(string name, string? description)
  {
    if (IsSystem)
      return Result.Error("System roles cannot be modified.");

    var validationErrors = new List<ValidationError>();
    ValidateName(name, validationErrors);

    if (description is not null && description.Trim().Length > RoleConstraints.DESCRIPTION_MAX_LENGTH)
      validationErrors.Add(new ValidationError(nameof(Description), $"cannot be longer than {RoleConstraints.DESCRIPTION_MAX_LENGTH} characters"));

    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    Name = name.Trim();
    Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    return Result.Success();
  }

  public static string NormalizeName(string name) => name.Trim().ToUpperInvariant();

  public void SetPermissions(IEnumerable<string> permissionCodes)
  {
    var distinctCodes = permissionCodes.Distinct(StringComparer.Ordinal).ToList();

    foreach (var permission in permissions.Where(p => !distinctCodes.Contains(p.PermissionCode)).ToList())
      permissions.Remove(permission);

    foreach (var code in distinctCodes)
      AddPermissionIfMissing(code);
  }

  public void AddPermissionIfMissing(string permissionCode)
  {
    if (permissions.Any(p => p.PermissionCode == permissionCode))
      return;

    permissions.Add(RolePermission.Create(Id, permissionCode));
  }

  private static void ValidateName(string name, List<ValidationError> validationErrors)
  {
    if (string.IsNullOrWhiteSpace(name))
      validationErrors.Add(new ValidationError(nameof(Name), "cannot be null or empty"));
    else if (name.Trim().Length < RoleConstraints.NAME_MIN_LENGTH)
      validationErrors.Add(new ValidationError(nameof(Name), $"must be at least {RoleConstraints.NAME_MIN_LENGTH} characters"));
    else if (name.Trim().Length > RoleConstraints.NAME_MAX_LENGTH)
      validationErrors.Add(new ValidationError(nameof(Name), $"cannot be longer than {RoleConstraints.NAME_MAX_LENGTH} characters"));
  }
}

public static class RoleConstraints
{
  public const int NAME_MIN_LENGTH = 2;
  public const int NAME_MAX_LENGTH = 100;
  public const int DESCRIPTION_MAX_LENGTH = 500;
  public const string AdministratorRoleName = "Administrator";
  public const string AdministratorDescription = "System administrator with full access.";
  public const string UserRoleName = "User";
  public const string UserDescription = "Default role for registered users.";
}
