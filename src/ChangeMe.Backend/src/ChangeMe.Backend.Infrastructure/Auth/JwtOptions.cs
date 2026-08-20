namespace ChangeMe.Backend.Infrastructure.Auth;

public sealed class JwtOptions
{
  public string Issuer { get; set; } = string.Empty;
  public string Audience { get; set; } = string.Empty;
  public string SigningKey { get; set; } = string.Empty;
  public int ExpirationMinutes { get; set; } = 30;
  public int SessionLifetimeDays { get; set; } = 14;
}
