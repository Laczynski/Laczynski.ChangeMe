using Microsoft.Extensions.Options;
using Npgsql;

namespace Template.Backend.Infrastructure.Configurations;

public sealed class ConnectionStringsOptionsValidator : IValidateOptions<ConnectionStringsOptions>
{
  public ValidateOptionsResult Validate(string? name, ConnectionStringsOptions options)
  {
    if (string.IsNullOrWhiteSpace(options.DefaultConnection))
    {
      return ValidateOptionsResult.Fail(
        "ConnectionStrings.DefaultConnection is required.");
    }

    try
    {
      _ = new NpgsqlConnectionStringBuilder(options.DefaultConnection);
      return ValidateOptionsResult.Success;
    }
    catch (ArgumentException)
    {
      return ValidateOptionsResult.Fail(
        "ConnectionStrings.DefaultConnection must be a valid PostgreSQL connection string.");
    }
  }

  public static string GetValidatedDefaultConnection(IConfiguration configuration)
  {
    return OptionsValidation.GetValidated(
        configuration,
        ConnectionStringsOptions.SectionName,
        new ConnectionStringsOptionsValidator())
      .DefaultConnection;
  }
}
