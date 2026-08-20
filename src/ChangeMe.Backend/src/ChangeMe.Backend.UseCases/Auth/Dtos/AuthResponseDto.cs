namespace ChangeMe.Backend.UseCases.Auth.Dtos;

public sealed record AuthResponseDto(
  Guid UserId,
  string FirstName,
  string LastName,
  string Email,
  Guid SessionId,
  string Token,
  DateTime ExpiresAtUtc,
  string RefreshToken,
  DateTime RefreshTokenExpiresAtUtc,
  IReadOnlyList<string> Permissions);
