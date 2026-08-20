namespace ChangeMe.Backend.Infrastructure.Persistence;

public sealed class InitialAdministratorOptions
{
  public const string SectionName = nameof(InitialAdministratorOptions);

  public string Email { get; set; } = string.Empty;
  public string Password { get; set; } = string.Empty;
  public string FirstName { get; set; } = string.Empty;
  public string LastName { get; set; } = string.Empty;
}
