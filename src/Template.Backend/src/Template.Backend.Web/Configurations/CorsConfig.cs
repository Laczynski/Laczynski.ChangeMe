using Template.Backend.Infrastructure.Configurations;

namespace Template.Backend.Web.Configurations;

public static class CorsConfig
{
  public const string CorsPolicyName = "CorsPolicy";

  public static IServiceCollection AddCors(this IServiceCollection services, WebApplicationBuilder builder)
  {
    var corsOptions = builder.Configuration
      .GetSection(CorsOptions.SectionName)
      .Get<CorsOptions>() ?? new CorsOptions();

    OptionsValidation.ThrowIfInvalid(
      new CorsOptionsValidator(),
      corsOptions,
      CorsOptions.SectionName);

    services.AddCors(options =>
    {
      options.AddPolicy(name: CorsPolicyName,
              policy =>
              {
                policy.WithOrigins(corsOptions.AllowedOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
              });
    });

    return services;
  }
}
