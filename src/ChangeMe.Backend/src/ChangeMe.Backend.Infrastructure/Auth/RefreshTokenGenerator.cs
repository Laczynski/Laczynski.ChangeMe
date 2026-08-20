using System.Security.Cryptography;
using System.Text;
using ChangeMe.Backend.Domain.Aggregates.Sessions;

namespace ChangeMe.Backend.Infrastructure.Auth;

public static class RefreshTokenGenerator
{
  public static string CreateToken()
  {
    var bytes = RandomNumberGenerator.GetBytes(SessionConstraints.REFRESH_TOKEN_BYTES);
    return Convert.ToBase64String(bytes);
  }

  public static string HashToken(string refreshToken)
  {
    var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
    return Convert.ToHexString(bytes);
  }
}
