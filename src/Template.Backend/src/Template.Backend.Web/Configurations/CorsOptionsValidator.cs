using Microsoft.Extensions.Options;

namespace Template.Backend.Web.Configurations;

public sealed class CorsOptionsValidator : IValidateOptions<CorsOptions>
{
  public ValidateOptionsResult Validate(string? name, CorsOptions options)
  {
    if (options.AllowedOrigins is null)
      return ValidateOptionsResult.Fail("CorsOptions.AllowedOrigins must not be null.");

    var failures = options.AllowedOrigins
      .Select((origin, index) => new { Origin = origin, Index = index })
      .Where(item => !IsValidOrigin(item.Origin))
      .Select(item =>
        $"CorsOptions.AllowedOrigins[{item.Index}] must be an absolute HTTP or HTTPS origin without a path.")
      .ToList();

    return failures.Count == 0
      ? ValidateOptionsResult.Success
      : ValidateOptionsResult.Fail(failures);
  }

  private static bool IsValidOrigin(string? origin)
  {
    if (string.IsNullOrWhiteSpace(origin)
        || !string.Equals(origin, origin.Trim(), StringComparison.Ordinal)
        || origin.EndsWith("/", StringComparison.Ordinal)
        || !Uri.TryCreate(origin, UriKind.Absolute, out var uri))
    {
      return false;
    }

    return (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
      && !string.IsNullOrWhiteSpace(uri.Host)
      && uri.AbsolutePath == "/"
      && string.IsNullOrEmpty(uri.Query)
      && string.IsNullOrEmpty(uri.Fragment)
      && string.IsNullOrEmpty(uri.UserInfo);
  }
}
