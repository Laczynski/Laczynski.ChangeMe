using ChangeMe.Backend.UseCases.Users.Dtos;

namespace ChangeMe.Backend.UseCases.Auth.Dtos;

public sealed record MyAccountDto(
  Guid Id,
  string FirstName,
  string LastName,
  string Email,
  DateTime MemberSince,
  long Version,
  IReadOnlyList<UserRoleSummaryDto> Roles,
  IReadOnlyList<EffectivePermissionDto> EffectivePermissions);
