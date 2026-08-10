using System.Threading.RateLimiting;
using Microsoft.Extensions.Options;
using Template.Backend.UseCases.Auth.Utils;

namespace Template.Backend.Web.Configurations;

public static class RateLimitingConfig
{
  public const string AuthPolicyName = "auth";

  public const string ApiPolicyName = "api";

  public static IServiceCollection AddRateLimiting(this IServiceCollection services, WebApplicationBuilder builder)
  {
    services.Configure<RateLimitingOptions>(builder.Configuration.GetSection(RateLimitingOptions.SectionName));

    services.AddRateLimiter(rateLimiterOptions =>
    {
      rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

      rateLimiterOptions.OnRejected = async (context, cancellationToken) =>
      {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
          context.HttpContext.Response.Headers.RetryAfter =
            ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString();
        }

        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync(
          "Too many requests. Please try again later.",
          cancellationToken);
      };

      rateLimiterOptions.AddPolicy(AuthPolicyName, context =>
      {
        var options = context.RequestServices.GetRequiredService<IOptionsMonitor<RateLimitingOptions>>().CurrentValue;
        return CreateFixedWindowPartition(context, options, options.AuthPermitLimit, options.AuthWindowSeconds);
      });

      rateLimiterOptions.AddPolicy(ApiPolicyName, context =>
      {
        var options = context.RequestServices.GetRequiredService<IOptionsMonitor<RateLimitingOptions>>().CurrentValue;
        return CreateFixedWindowPartition(context, options, options.ApiPermitLimit, options.ApiWindowSeconds);
      });

      rateLimiterOptions.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
      {
        if (ShouldBypassGlobalRateLimit(context))
          return RateLimitPartition.GetNoLimiter("bypass");

        var options = context.RequestServices.GetRequiredService<IOptionsMonitor<RateLimitingOptions>>().CurrentValue;
        return CreateFixedWindowPartition(context, options, options.ApiPermitLimit, options.ApiWindowSeconds);
      });
    });

    return services;
  }

  public static WebApplication UseRateLimiting(this WebApplication app)
  {
    app.UseRateLimiter();
    return app;
  }

  private static bool ShouldBypassGlobalRateLimit(HttpContext context)
  {
    return context.Request.Path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase);
  }

  private static RateLimitPartition<string> CreateFixedWindowPartition(
    HttpContext context,
    RateLimitingOptions options,
    int permitLimit,
    int windowSeconds)
  {
    var partitionKey = AuthSessionUtils.GetClientIpAddress(context) ?? "unknown";
    var effectivePermitLimit = options.Enabled ? permitLimit : int.MaxValue;

    return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
    {
      PermitLimit = effectivePermitLimit,
      Window = TimeSpan.FromSeconds(Math.Max(1, windowSeconds)),
      QueueLimit = 0,
      AutoReplenishment = true
    });
  }
}
