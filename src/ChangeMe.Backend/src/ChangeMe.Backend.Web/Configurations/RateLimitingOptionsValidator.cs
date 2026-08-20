using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.Web.Configurations;

public sealed class RateLimitingOptionsValidator : IValidateOptions<RateLimitingOptions>
{
  public ValidateOptionsResult Validate(string? name, RateLimitingOptions options)
  {
    var failures = new List<string>();

    if (options.AuthPermitLimit <= 0)
      failures.Add("RateLimitingOptions.AuthPermitLimit must be greater than zero.");

    if (options.AuthWindowSeconds <= 0)
      failures.Add("RateLimitingOptions.AuthWindowSeconds must be greater than zero.");

    if (options.ApiPermitLimit <= 0)
      failures.Add("RateLimitingOptions.ApiPermitLimit must be greater than zero.");

    if (options.ApiWindowSeconds <= 0)
      failures.Add("RateLimitingOptions.ApiWindowSeconds must be greater than zero.");

    return failures.Count == 0
      ? ValidateOptionsResult.Success
      : ValidateOptionsResult.Fail(failures);
  }
}
