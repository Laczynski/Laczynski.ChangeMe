namespace ChangeMe.Backend.Domain.Aggregates.Users.Interfaces;

public interface IPasswordHasher
{
  string HashPassword(string password);
  bool VerifyPassword(string hashedPassword, string providedPassword);
}
