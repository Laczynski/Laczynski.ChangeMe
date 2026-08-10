using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace Template.Backend.Infrastructure.Configurations;

public sealed class HangfireOptions
{
  public const string SectionName = nameof(HangfireOptions);

  public string DashboardPath { get; set; } = "/hangfire";

  public bool DashboardEnabled { get; set; }

  public bool ServerEnabled { get; set; } = true;
}

public static class HangfireConfig
{
  public static IServiceCollection AddHangfire(this IServiceCollection services, WebApplicationBuilder builder, ILogger logger)
  {
    services.Configure<HangfireOptions>(builder.Configuration.GetSection(HangfireOptions.SectionName));

    var hangfireOptions = builder.Configuration
      .GetSection(HangfireOptions.SectionName)
      .Get<HangfireOptions>() ?? new HangfireOptions();

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
      var keys = string.Join(", ",
          builder.Configuration.GetSection("ConnectionStrings").GetChildren().Select(c => c.Key));
      throw new InvalidOperationException(
          $"Connection string 'DefaultConnection' is not configured. Available connection string keys: {keys}");
    }

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

    app.UseHangfireDashboard(options.DashboardPath ?? "/hangfire");
    return app;
  }
}
