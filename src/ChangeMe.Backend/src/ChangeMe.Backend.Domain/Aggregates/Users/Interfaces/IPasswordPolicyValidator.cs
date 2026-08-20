namespace ChangeMe.Backend.Domain.Aggregates.Users.Interfaces;

public interface IPasswordPolicyValidator
{
  IReadOnlyList<ValidationError> Validate(string password, string propertyName = "Password");
}
