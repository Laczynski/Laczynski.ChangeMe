using Microsoft.AspNetCore.Identity;

namespace ChangeMe.Backend.Infrastructure.Auth;

public class PasswordHasherAdapter : IPasswordHasher
{
  private readonly PasswordHasher<object> passwordHasher = new();
  private static readonly object PasswordHasherUser = new();

  public string HashPassword(string password) => passwordHasher.HashPassword(PasswordHasherUser, password);

  public bool VerifyPassword(string hashedPassword, string providedPassword)
  {
    var verificationResult = passwordHasher.VerifyHashedPassword(
      PasswordHasherUser,
      hashedPassword,
      providedPassword);

    return verificationResult is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
  }
}
