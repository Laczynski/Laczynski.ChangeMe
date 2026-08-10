using Template.Backend.Domain.Aggregates.Users;
using Template.Backend.UseCases.Users.Dtos;

namespace Template.Backend.UseCases.Users.Utils;

public static class UsersMappingExtensions
{
  public static UserDetailsDto ToDetailsDto(
    this User user,
    DateTime? lastSignInAt,
    IReadOnlyList<UserRoleSummaryDto> roles,
    IReadOnlyList<EffectivePermissionDto> effectivePermissions) =>
    new()
    {
      Id = user.Id,
      FirstName = user.FirstName,
      LastName = user.LastName,
      Email = user.Email,
      Deactivated = user.Deactivated,
      DeactivatedAt = user.DeactivatedAt,
      Status = UsersStatusUtils.ComputeStatus(user),
      MemberSince = user.CreatedAt,
      LastSignInAt = lastSignInAt,
      Version = user.Version,
      Roles = roles,
      EffectivePermissions = effectivePermissions
    };
}
