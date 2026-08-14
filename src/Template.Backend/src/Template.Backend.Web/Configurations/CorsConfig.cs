using Template.Backend.Infrastructure.Configurations;

namespace Template.Backend.Web.Configurations;

public static class CorsConfig
{
  public const string CorsPolicyName = "CorsPolicy";

  public static IServiceCollection AddCors(this IServiceCollection services, WebApplicationBuilder builder)
  {
    var corsOptions = OptionsValidation.GetValidated(
      builder.Configuration,
      CorsOptions.SectionName,
      new CorsOptionsValidator());

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
