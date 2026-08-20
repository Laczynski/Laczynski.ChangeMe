using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.Infrastructure.Configurations;

public static class HangfireConfig
{
  public static IServiceCollection AddHangfire(this IServiceCollection services, WebApplicationBuilder builder, ILogger logger)
  {
    var hangfireOptions = OptionsValidation.GetValidated(
      builder.Configuration,
      HangfireOptions.SectionName,
      new HangfireOptionsValidator());

    var connectionString = ConnectionStringsOptionsValidator.GetValidatedDefaultConnection(builder.Configuration);

    services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString)));

    if (hangfireOptions.ServerEnabled)
      services.AddHangfireServer();

    logger.LogInformation(
      "{Project} services configured (server {ServerState})",
      "Hangfire",
      hangfireOptions.ServerEnabled ? "enabled" : "disabled");
    return services;
  }

  public static WebApplication UseHangfireDashboard(this WebApplication app)
  {
    var options = app.Services.GetRequiredService<IOptions<HangfireOptions>>().Value;
    if (!options.DashboardEnabled)
      return app;

    app.UseHangfireDashboard(options.DashboardPath);
    return app;
  }
}
