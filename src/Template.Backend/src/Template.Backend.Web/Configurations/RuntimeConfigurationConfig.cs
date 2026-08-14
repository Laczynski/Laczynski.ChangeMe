using Microsoft.Extensions.Options;
using Template.Backend.Infrastructure.Auth;
using Template.Backend.Infrastructure.Configurations;
using Template.Backend.Infrastructure.Email;
using Template.Backend.Infrastructure.FileStorage;
using Template.Backend.Infrastructure.Persistence;
using Template.Backend.UseCases.Notifications.Services;

namespace Template.Backend.Web.Configurations;

public static class RuntimeConfigurationConfig
{
  public static IServiceCollection AddRuntimeConfiguration(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    services.AddValidatedOptions<ConnectionStringsOptions, ConnectionStringsOptionsValidator>(
      configuration, ConnectionStringsOptions.SectionName);
    services.AddValidatedOptions<AuthOptions, AuthOptionsValidator>(
      configuration, AuthOptions.SectionName);
    services.AddValidatedOptions<EmailOptions, EmailOptionsValidator>(
      configuration, EmailOptions.SectionName);
    services.AddValidatedOptions<RateLimitingOptions, RateLimitingOptionsValidator>(
      configuration, RateLimitingOptions.SectionName);
    services.AddValidatedOptions<FileStorageOptions, FileStorageOptionsValidator>(
      configuration, FileStorageOptions.SectionName);
    services.AddValidatedOptions<NotificationRetentionOptions, NotificationRetentionOptionsValidator>(
      configuration, NotificationRetentionOptions.SectionName);
    services.AddValidatedOptions<HangfireOptions, HangfireOptionsValidator>(
      configuration, HangfireOptions.SectionName);
    services.AddValidatedOptions<CorsOptions, CorsOptionsValidator>(
      configuration, CorsOptions.SectionName);
    services.AddValidatedOptions<InitialAdministratorOptions, InitialAdministratorOptionsValidator>(
      configuration, InitialAdministratorOptions.SectionName);

    return services;
  }

  public static WebApplication ValidateRuntimeConfiguration(this WebApplication app)
  {
    Validate<ConnectionStringsOptions>(app.Services);
    Validate<AuthOptions>(app.Services);
    Validate<EmailOptions>(app.Services);
    Validate<RateLimitingOptions>(app.Services);
    Validate<FileStorageOptions>(app.Services);
    Validate<NotificationRetentionOptions>(app.Services);
    Validate<HangfireOptions>(app.Services);
    Validate<CorsOptions>(app.Services);
    Validate<InitialAdministratorOptions>(app.Services);
    return app;
  }

  private static void Validate<TOptions>(IServiceProvider services)
    where TOptions : class
  {
    _ = services.GetRequiredService<IOptions<TOptions>>().Value;
  }
}
