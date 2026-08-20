using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.Infrastructure.Email;
using ChangeMe.Backend.Infrastructure.FileStorage;

namespace ChangeMe.Backend.Infrastructure.Configurations;

public static class ServicesConfig
{
  public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, ILogger logger)
  {
    services.AddScoped<IEmailService, EmailService>();
    services.AddSingleton<IPasswordPolicyValidator, PasswordPolicyValidator>();
    services.AddSingleton<IPasswordHasher, PasswordHasherAdapter>();
    services.AddSingleton<ISessionLifetimeService, SessionLifetimeService>();
    services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
    services.AddScoped<IUserAccessor, UserAccessor>();
    services.AddSingleton<FileContentInspectorProvider>();
    services.AddSingleton<IFileContentValidator, FileContentValidator>();
    services.AddScoped<IFileStorageService, LocalFileStorageService>();
    logger.LogInformation("{Project} services configured", "Infrastructure");
    return services;
  }
}
