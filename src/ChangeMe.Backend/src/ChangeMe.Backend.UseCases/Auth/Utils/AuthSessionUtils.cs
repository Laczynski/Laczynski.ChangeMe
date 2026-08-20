using ChangeMe.Backend.Domain.Aggregates.Sessions;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using Microsoft.AspNetCore.Http;

namespace ChangeMe.Backend.UseCases.Auth.Utils;

public static class AuthSessionUtils
{
  public const string InvalidCredentialsMessage = "Invalid email or password.";
  public const string DeactivatedAccountMessage = "This account has been deactivated. Contact an administrator.";
  public const string DuplicateEmailMessage = "An account with this email already exists.";
  public const string PermissionDeniedMessage = "You do not have permission to perform this action.";

  public static async Task<Result<AuthResponseDto>> CreateAuthResponseAsync(
    ApplicationDbContext context,
    IJwtTokenGenerator jwtTokenGenerator,
    User user,
    UserSession session,
    string refreshToken,
    CancellationToken cancellationToken)
  {
    var permissions = await PermissionResolver.GetEffectivePermissionsAsync(context, user.Id, cancellationToken);
    var accessToken = jwtTokenGenerator.GenerateToken(user, session.Id, permissions);

    return Result.Success(new AuthResponseDto(
      user.Id,
      user.FirstName,
      user.LastName,
      user.Email,
      session.Id,
      accessToken.Token,
      accessToken.ExpiresAtUtc,
      refreshToken,
      session.RefreshTokenExpiresAtUtc,
      permissions));
  }

  public static string? GetClientIpAddress(HttpContext? httpContext)
  {
    if (httpContext is null)
      return null;

    var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
    if (!string.IsNullOrWhiteSpace(forwardedFor))
      return forwardedFor.Split(',')[0].Trim();

    return httpContext.Connection.RemoteIpAddress?.ToString();
  }
}
