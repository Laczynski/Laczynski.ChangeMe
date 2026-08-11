using Microsoft.Extensions.Options;
using Template.Backend.Infrastructure.Auth;

namespace Template.Backend.UnitTests.Support;

internal sealed class FakeUserAccessor : IUserAccessor
{
  public Guid? UserId { get; set; }
  public Guid? SessionId { get; set; }

  public bool HasPermission(string permissionCode) => true;
}

internal static class TestAuthOptions
{
  public static IOptions<AuthOptions> Create() =>
    Options.Create(new AuthOptions
    {
      Jwt = new JwtOptions
      {
        Issuer = "Template.Tests",
        Audience = "Template.Tests",
        SigningKey = "Integration-Tests-Signing-Key-Needs-32-Chars",
        ExpirationMinutes = 60,
        SessionLifetimeDays = 14
      }
    });
}
