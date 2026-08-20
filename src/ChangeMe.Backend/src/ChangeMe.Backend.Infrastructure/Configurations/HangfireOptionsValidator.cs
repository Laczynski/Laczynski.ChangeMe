using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.Infrastructure.Configurations;

public sealed class HangfireOptionsValidator : IValidateOptions<HangfireOptions>
{
  public ValidateOptionsResult Validate(string? name, HangfireOptions options)
  {
    return !string.IsNullOrWhiteSpace(options.DashboardPath)
      && options.DashboardPath.StartsWith("/", StringComparison.Ordinal)
        ? ValidateOptionsResult.Success
        : ValidateOptionsResult.Fail("HangfireOptions.DashboardPath must start with '/'.");
  }
}
