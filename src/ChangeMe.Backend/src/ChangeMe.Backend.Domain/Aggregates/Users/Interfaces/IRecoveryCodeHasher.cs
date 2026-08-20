namespace ChangeMe.Backend.Domain.Aggregates.Users.Interfaces;

public interface IRecoveryCodeHasher
{
  string Hash(string recoveryCode);

  bool Verify(string recoveryCode, string codeHash);
}
