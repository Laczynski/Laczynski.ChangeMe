using ChangeMe.Backend.Domain.Aggregates.Users;

namespace ChangeMe.Backend.Infrastructure.Auth;

public sealed class AuthOptions
{
  public const string SectionName = nameof(AuthOptions);

  public string FrontendBaseUrl { get; set; } = "http://localhost:4200";

  public JwtOptions Jwt { get; set; } = new();

  public PasswordPolicyOptions PasswordPolicy { get; set; } = new();
}

public sealed class PasswordPolicyOptions
{
  public int MinimumLength { get; set; } = UserConstraints.PASSWORD_MIN_LENGTH;

  public int MaximumLength { get; set; } = UserConstraints.PASSWORD_MAX_LENGTH;

  public bool RequireUppercase { get; set; } = true;

  public bool RequireLowercase { get; set; } = true;

  public bool RequireDigit { get; set; } = true;

  public bool RequireSpecialCharacter { get; set; }
}
