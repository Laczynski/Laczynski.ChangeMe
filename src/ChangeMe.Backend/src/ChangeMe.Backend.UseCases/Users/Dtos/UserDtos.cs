using DataGrid.Abstractions;

namespace ChangeMe.Backend.UseCases.Users.Dtos;

public sealed record UserListItemDto
{
  public Guid Id { get; init; }

  [GridSearchable]
  public string FirstName { get; init; } = string.Empty;

  [GridSearchable]
  public string LastName { get; init; } = string.Empty;

  [GridSearchable]
  public string Email { get; init; } = string.Empty;
  public bool Deactivated { get; init; }
  public UserMembershipStatus Status { get; init; }

  [GridIgnore]
  public IReadOnlyList<string> RoleNames { get; init; } = [];
  public DateTime? LastSignInAt { get; init; }
  public DateTime CreatedAt { get; init; }
}

public sealed record UserRoleSummaryDto(Guid Id, string Name, bool IsSystem);

public sealed record EffectivePermissionDto(
  string Code,
  string Label,
  string Description,
  string Group,
  IReadOnlyList<string> FromRoleNames);

public sealed record UserDetailsDto
{
  public Guid Id { get; init; }
  public string FirstName { get; init; } = string.Empty;
  public string LastName { get; init; } = string.Empty;
  public string Email { get; init; } = string.Empty;
  public bool Deactivated { get; init; }
  public DateTime? DeactivatedAt { get; init; }
  public UserMembershipStatus Status { get; init; }
  public DateTime MemberSince { get; init; }
  public DateTime? LastSignInAt { get; init; }
  public long Version { get; init; }
  public IReadOnlyList<UserRoleSummaryDto> Roles { get; init; } = [];
  public IReadOnlyList<EffectivePermissionDto> EffectivePermissions { get; init; } = [];
}

public sealed record RoleAssignmentOptionDto(Guid Id, string Name, bool IsSystem);

public sealed record AdminUserSessionDto(
  Guid Id,
  string DeviceBrowserLabel,
  string SignInMethod,
  string? IpAddress,
  DateTime SignedInAt,
  DateTime LastActivityAt);
