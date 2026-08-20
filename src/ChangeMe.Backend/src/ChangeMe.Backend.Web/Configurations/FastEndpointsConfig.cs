using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using NSwag;

namespace ChangeMe.Backend.Web.Configurations;

public static class FastEndpointsConfig
{
  public static IServiceCollection AddFastEndpointsWithSwagger(this IServiceCollection services, IConfiguration configuration)
  {
    services.Configure<SwaggerOptions>(configuration.GetSection(SwaggerOptions.SectionName));
    services.ConfigureHttpJsonOptions(options =>
    {
      options.SerializerOptions.PropertyNameCaseInsensitive = true;
      options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

    services.AddFastEndpoints()
            .SwaggerDocument(o =>
            {
              o.MaxEndpointVersion = ApiVersionConfig.CurrentVersion;
              o.ShortSchemaNames = true;
              o.EnableJWTBearerAuth = false;
              o.DocumentSettings = settings =>
                  {
                    settings.DocumentName = $"v{ApiVersionConfig.CurrentVersion}";
                    settings.Title = "Template API";
                    settings.Version = $"{ApiVersionConfig.Prefix}{ApiVersionConfig.CurrentVersion}";
                    settings.AddAuth("Bearer", new OpenApiSecurityScheme
                    {
                      Type = OpenApiSecuritySchemeType.Http,
                      Scheme = JwtBearerDefaults.AuthenticationScheme,
                      BearerFormat = "JWT",
                    });
                  };
            });
    return services;
  }

  public static WebApplication UseFastEndpointsWithSwagger(this WebApplication app)
  {
    var endpointBuilder = app.UseFastEndpoints(config =>
    {
      config.Endpoints.RoutePrefix = "api";
      config.Versioning.Prefix = ApiVersionConfig.Prefix;
      config.Versioning.PrependToRoute = true;
      config.Versioning.DefaultVersion = ApiVersionConfig.CurrentVersion;
      config.Serializer.Options.PropertyNameCaseInsensitive = true;
      config.Serializer.Options.Converters.Add(new JsonStringEnumConverter());
    });

    var swaggerEnabled = app.Services.GetRequiredService<IOptions<SwaggerOptions>>().Value.Enabled;
    if (swaggerEnabled)
      endpointBuilder.UseSwaggerGen();

    return app;
  }
}
