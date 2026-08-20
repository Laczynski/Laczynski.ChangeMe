using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ChangeMe.Backend.Domain.Aggregates.Sessions;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ChangeMe.Backend.Infrastructure.Auth;

public class JwtTokenGenerator(IOptions<AuthOptions> options) : IJwtTokenGenerator
{
  public const string SessionIdClaimType = "sid";

  private JwtOptions Jwt => options.Value.Jwt;

  public AccessTokenResult GenerateToken(User user, Guid sessionId, IReadOnlyList<string> permissions)
  {
    var expiresAtUtc = DateTime.UtcNow.AddMinutes(
      Jwt.ExpirationMinutes > 0
        ? Jwt.ExpirationMinutes
        : SessionConstraints.ACCESS_TOKEN_LIFETIME_MINUTES);

    var claims = new List<Claim>
    {
      new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
      new(JwtRegisteredClaimNames.Email, user.Email),
      new(JwtRegisteredClaimNames.Sid, sessionId.ToString()),
      new(ClaimTypes.NameIdentifier, user.Id.ToString()),
      new(ClaimTypes.Email, user.Email),
      new(SessionIdClaimType, sessionId.ToString())
    };

    claims.AddRange(permissions.Select(permission => new Claim(PermissionClaimTypes.Permission, permission)));

    var credentials = new SigningCredentials(
      new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Jwt.SigningKey)),
      SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
      issuer: Jwt.Issuer,
      audience: Jwt.Audience,
      claims: claims.ToArray(),
      expires: expiresAtUtc,
      signingCredentials: credentials);

    return new AccessTokenResult(
      new JwtSecurityTokenHandler().WriteToken(token),
      expiresAtUtc);
  }
}
