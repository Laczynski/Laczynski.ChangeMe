using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ChangeMe.Backend.Domain.Authorization;
using Microsoft.AspNetCore.Http;

namespace ChangeMe.Backend.Infrastructure.Auth;

public class UserAccessor(IHttpContextAccessor httpContextAccessor) : IUserAccessor
{
  public Guid? UserId
  {
    get
    {
      var userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? httpContextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
      return Guid.TryParse(userId, out var parsedUserId) ? parsedUserId : null;
    }
  }

  public Guid? SessionId
  {
    get
    {
      var sessionId = httpContextAccessor.HttpContext?.User.FindFirst(JwtTokenGenerator.SessionIdClaimType)?.Value;
      return Guid.TryParse(sessionId, out var parsedSessionId) ? parsedSessionId : null;
    }
  }

  public bool HasPermission(string permissionCode) =>
    httpContextAccessor.HttpContext?.User
      .HasClaim(PermissionClaimTypes.Permission, permissionCode) ?? false;
}
