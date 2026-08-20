namespace ChangeMe.Backend.Domain.Aggregates.Roles.Entities;

public class RolePermission
{
  private RolePermission() { }

  public Guid RoleId { get; private set; }
  public Role Role { get; private set; } = null!;
  public string PermissionCode { get; private set; } = string.Empty;

  public static RolePermission Create(Guid roleId, string permissionCode) =>
    new()
    {
      RoleId = roleId,
      PermissionCode = permissionCode
    };
}
