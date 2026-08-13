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
    services.AddSingleton<IValidateOptions<ConnectionStringsOptions>, ConnectionStringsOptionsValidator>();
    services.AddOptions<ConnectionStringsOptions>()
      .Bind(configuration.GetSection(ConnectionStringsOptions.SectionName))
      .ValidateOnStart();

    services.AddSingleton<IValidateOptions<AuthOptions>, AuthOptionsValidator>();
    services.AddOptions<AuthOptions>()
      .Bind(configuration.GetSection(AuthOptions.SectionName))
      .ValidateOnStart();

    services.AddSingleton<IValidateOptions<EmailOptions>, EmailOptionsValidator>();
    services.AddOptions<EmailOptions>()
      .Bind(configuration.GetSection(EmailOptions.SectionName))
      .ValidateOnStart();

    services.AddSingleton<IValidateOptions<RateLimitingOptions>, RateLimitingOptionsValidator>();
    services.AddOptions<RateLimitingOptions>()
      .Bind(configuration.GetSection(RateLimitingOptions.SectionName))
      .ValidateOnStart();

    services.AddSingleton<IValidateOptions<FileStorageOptions>, FileStorageOptionsValidator>();
    services.AddOptions<FileStorageOptions>()
      .Bind(configuration.GetSection(FileStorageOptions.SectionName))
      .ValidateOnStart();

    services.AddSingleton<IValidateOptions<NotificationRetentionOptions>, NotificationRetentionOptionsValidator>();
    services.AddOptions<NotificationRetentionOptions>()
      .Bind(configuration.GetSection(NotificationRetentionOptions.SectionName))
      .ValidateOnStart();

    services.AddSingleton<IValidateOptions<HangfireOptions>, HangfireOptionsValidator>();
    services.AddOptions<HangfireOptions>()
      .Bind(configuration.GetSection(HangfireOptions.SectionName))
      .ValidateOnStart();

    services.AddSingleton<IValidateOptions<CorsOptions>, CorsOptionsValidator>();
    services.AddOptions<CorsOptions>()
      .Bind(configuration.GetSection(CorsOptions.SectionName))
      .ValidateOnStart();

    services.AddSingleton<IValidateOptions<InitialAdministratorOptions>, InitialAdministratorOptionsValidator>();
    services.AddOptions<InitialAdministratorOptions>()
      .Bind(configuration.GetSection(InitialAdministratorOptions.SectionName))
      .ValidateOnStart();

    services.AddSingleton<RuntimeConfigurationValidation>();
    return services;
  }

  public static WebApplication ValidateRuntimeConfiguration(this WebApplication app)
  {
    app.Services.GetRequiredService<RuntimeConfigurationValidation>().Validate();
    return app;
  }

  private sealed class RuntimeConfigurationValidation(
    IOptions<ConnectionStringsOptions> connectionStrings,
    IOptions<AuthOptions> auth,
    IOptions<EmailOptions> email,
    IOptions<RateLimitingOptions> rateLimiting,
    IOptions<FileStorageOptions> fileStorage,
    IOptions<NotificationRetentionOptions> notificationRetention,
    IOptions<HangfireOptions> hangfire,
    IOptions<CorsOptions> cors,
    IOptions<InitialAdministratorOptions> initialAdministrator)
  {
    public void Validate()
    {
      _ = connectionStrings.Value;
      _ = auth.Value;
      _ = email.Value;
      _ = rateLimiting.Value;
      _ = fileStorage.Value;
      _ = notificationRetention.Value;
      _ = hangfire.Value;
      _ = cors.Value;
      _ = initialAdministrator.Value;
    }
  }
}
