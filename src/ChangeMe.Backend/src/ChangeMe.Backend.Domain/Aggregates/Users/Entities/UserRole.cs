using ChangeMe.Backend.Domain.Aggregates.Roles;

namespace ChangeMe.Backend.Domain.Aggregates.Users.Entities;

public class UserRole
{
  private UserRole() { }

  public Guid UserId { get; private set; }
  public User User { get; private set; } = null!;
  public Guid RoleId { get; private set; }
  public Role Role { get; private set; } = null!;

  public static UserRole Create(Guid userId, Guid roleId) =>
    new()
    {
      UserId = userId,
      RoleId = roleId
    };
}
