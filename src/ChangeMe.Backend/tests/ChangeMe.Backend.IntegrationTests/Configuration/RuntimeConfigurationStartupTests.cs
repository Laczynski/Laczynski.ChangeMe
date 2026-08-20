using ChangeMe.Backend.Web.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.IntegrationTests.Configuration;

public sealed class RuntimeConfigurationStartupTests
{
  [Fact]
  public async Task ValidProductionShapedConfiguration_StartsHost()
  {
    using var host = CreateHost(CreateValidConfiguration());

    await host.StartAsync(TestContext.Current.CancellationToken);
    await host.StopAsync(TestContext.Current.CancellationToken);
  }

  [Theory]
  [MemberData(nameof(InvalidConfigurations))]
  public async Task InvalidConfiguration_PreventsHostStartup(
    IReadOnlyDictionary<string, string?> configuration,
    string expectedProperty)
  {
    using var host = CreateHost(configuration);

    var exception = await Assert.ThrowsAsync<OptionsValidationException>(
      () => host.StartAsync(TestContext.Current.CancellationToken));

    Assert.Contains(expectedProperty, exception.Message, StringComparison.Ordinal);
  }

  public static IEnumerable<object[]> InvalidConfigurations()
  {
    yield return Invalid("ConnectionStrings:DefaultConnection", "not-a-connection-string", "ConnectionStrings.DefaultConnection");
    yield return Invalid("AuthOptions:Jwt:SigningKey", "too-short", "AuthOptions.Jwt.SigningKey");
    yield return Invalid("EmailOptions:Port", "0", "EmailOptions.Port");
    var disabledRateLimiting = CreateValidConfiguration();
    disabledRateLimiting["RateLimitingOptions:Enabled"] = "false";
    disabledRateLimiting["RateLimitingOptions:ApiWindowSeconds"] = "0";
    yield return [disabledRateLimiting, "RateLimitingOptions.ApiWindowSeconds"];
    yield return Invalid("FileStorageOptions:CleanupCronExpression", "not a cron", "FileStorageOptions.CleanupCronExpression");
    yield return Invalid("NotificationRetentionOptions:AbsoluteRetentionDays", "10", "NotificationRetentionOptions.UnreadRetentionDays");
    yield return Invalid("HangfireOptions:DashboardPath", "hangfire", "HangfireOptions.DashboardPath");
    yield return Invalid("CorsOptions:AllowedOrigins:0", "https://app.example.com/path", "CorsOptions.AllowedOrigins[0]");
    yield return Invalid("InitialAdministratorOptions:Email", "admin@example.com", "InitialAdministratorOptions.Password");
  }

  private static object[] Invalid(string key, string value, string expectedProperty)
  {
    var configuration = CreateValidConfiguration();
    configuration[key] = value;
    return [configuration, expectedProperty];
  }

  private static IHost CreateHost(IReadOnlyDictionary<string, string?> configuration)
  {
    var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
    {
      DisableDefaults = true
    });
    builder.Configuration.AddInMemoryCollection(configuration);
    builder.Services.AddRuntimeConfiguration(builder.Configuration);
    return builder.Build();
  }

  private static Dictionary<string, string?> CreateValidConfiguration() => new()
  {
    ["ConnectionStrings:DefaultConnection"] = "Host=postgres;Database=Template;Username=template;Password=test-only",
    ["AuthOptions:FrontendBaseUrl"] = "https://app.example.com",
    ["AuthOptions:Jwt:Issuer"] = "ChangeMe",
    ["AuthOptions:Jwt:Audience"] = "ChangeMe.Client",
    ["AuthOptions:Jwt:SigningKey"] = "Production-Shaped-Test-Key-With-32-Bytes",
    ["AuthOptions:Jwt:ExpirationMinutes"] = "30",
    ["AuthOptions:Jwt:SessionLifetimeDays"] = "14",
    ["AuthOptions:PasswordPolicy:MinimumLength"] = "8",
    ["AuthOptions:PasswordPolicy:MaximumLength"] = "128",
    ["AuthOptions:PasswordPolicy:RequireUppercase"] = "true",
    ["AuthOptions:PasswordPolicy:RequireLowercase"] = "true",
    ["AuthOptions:PasswordPolicy:RequireDigit"] = "true",
    ["AuthOptions:PasswordPolicy:RequireSpecialCharacter"] = "true",
    ["EmailOptions:Host"] = "smtp.example.com",
    ["EmailOptions:Port"] = "587",
    ["EmailOptions:FromEmail"] = "no-reply@example.com",
    ["RateLimitingOptions:AuthPermitLimit"] = "10",
    ["RateLimitingOptions:AuthWindowSeconds"] = "60",
    ["RateLimitingOptions:ApiPermitLimit"] = "100",
    ["RateLimitingOptions:ApiWindowSeconds"] = "60",
    ["FileStorageOptions:RootPath"] = "/app/storage",
    ["FileStorageOptions:CleanupCronExpression"] = "0 * * * *",
    ["FileStorageOptions:CleanupConcurrentExecutionTimeoutSeconds"] = "3600",
    ["NotificationRetentionOptions:ReadRetentionDays"] = "30",
    ["NotificationRetentionOptions:UnreadRetentionDays"] = "90",
    ["NotificationRetentionOptions:AbsoluteRetentionDays"] = "180",
    ["NotificationRetentionOptions:CleanupCronExpression"] = "0 3 * * *",
    ["HangfireOptions:DashboardPath"] = "/hangfire",
    ["CorsOptions:AllowedOrigins:0"] = "https://app.example.com"
  };
}
