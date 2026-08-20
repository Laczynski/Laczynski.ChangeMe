using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.Infrastructure.Configurations;
using ChangeMe.Backend.Infrastructure.Email;
using ChangeMe.Backend.Infrastructure.FileStorage;
using ChangeMe.Backend.Infrastructure.Persistence;
using ChangeMe.Backend.IntegrationTests.Support;
using ChangeMe.Backend.IntegrationTests.Support.Fakes;
using ChangeMe.Backend.Web.Configurations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace ChangeMe.Backend.IntegrationTests.Fixtures;

public class BackendWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
  private readonly string fileStorageRootPath = Path.Combine(
    Path.GetTempPath(),
    "template-integration-tests",
    Guid.NewGuid().ToString("N"));
  private readonly Dictionary<string, string?> environmentOverrides = new();
  private readonly PostgreSqlContainer postgresContainer = new PostgreSqlBuilder("postgres:18")
    .Build();

  public async ValueTask InitializeAsync()
  {
    var cancellationToken = TestContext.Current.CancellationToken;

    await postgresContainer.StartAsync(cancellationToken);
    ApplyEnvironmentOverrides();

    using var client = CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    await DatabaseConfig.InitializeDatabaseAsync(Services, cancellationToken);
  }

  public new async ValueTask DisposeAsync()
  {
    var cancellationToken = TestContext.Current.CancellationToken;

    ClearEnvironmentOverrides();
    await postgresContainer.DisposeAsync().AsTask().WaitAsync(cancellationToken);
    await base.DisposeAsync();
    TryDeleteFileStorageRoot();
    GC.SuppressFinalize(this);
  }

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder.UseEnvironment("Testing");
    builder.ConfigureServices(services =>
    {
      var emailServiceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IEmailService));
      if (emailServiceDescriptor is not null)
      {
        services.Remove(emailServiceDescriptor);
      }

      services.AddSingleton<IEmailService, FakeEmailService>();
    });
  }

  private void ApplyEnvironmentOverrides()
  {
    environmentOverrides["ConnectionStrings__DefaultConnection"] = postgresContainer.GetConnectionString();
    environmentOverrides[$"{DatabaseOptions.SectionName}__ApplyMigrationsOnStartup"] = "false";
    environmentOverrides[$"{AuthOptions.SectionName}__Jwt__Issuer"] = "ChangeMe.Tests";
    environmentOverrides[$"{AuthOptions.SectionName}__Jwt__Audience"] = "ChangeMe.Tests";
    environmentOverrides[$"{AuthOptions.SectionName}__Jwt__SigningKey"] = "Integration-Tests-Signing-Key-Needs-32-Chars";
    environmentOverrides[$"{AuthOptions.SectionName}__Jwt__ExpirationMinutes"] = "60";
    environmentOverrides[$"{AuthOptions.SectionName}__Jwt__SessionLifetimeDays"] = "14";
    environmentOverrides[$"{AuthOptions.SectionName}__PasswordPolicy__RequireSpecialCharacter"] = "false";
    environmentOverrides[$"{InitialAdministratorOptions.SectionName}__Email"] = TestAuthHelper.SeededAdminEmail;
    environmentOverrides[$"{InitialAdministratorOptions.SectionName}__Password"] = TestAuthHelper.SeededAdminPassword;
    environmentOverrides[$"{InitialAdministratorOptions.SectionName}__FirstName"] = "Integration";
    environmentOverrides[$"{InitialAdministratorOptions.SectionName}__LastName"] = "Admin";
    environmentOverrides[$"{EmailOptions.SectionName}__Host"] = "localhost";
    environmentOverrides[$"{EmailOptions.SectionName}__Port"] = "1025";
    environmentOverrides[$"{EmailOptions.SectionName}__EnableSsl"] = "false";
    environmentOverrides[$"{EmailOptions.SectionName}__FromEmail"] = "tests@example.local";
    environmentOverrides[$"{EmailOptions.SectionName}__FromName"] = "Integration Tests";
    Directory.CreateDirectory(fileStorageRootPath);
    environmentOverrides[$"{FileStorageOptions.SectionName}__RootPath"] = fileStorageRootPath;
    environmentOverrides[$"{RateLimitingOptions.SectionName}__AuthPermitLimit"] = "1000";
    environmentOverrides[$"{RateLimitingOptions.SectionName}__ApiPermitLimit"] = "10000";

    foreach (var pair in environmentOverrides)
    {
      Environment.SetEnvironmentVariable(pair.Key, pair.Value);
    }
  }

  private void ClearEnvironmentOverrides()
  {
    foreach (var pair in environmentOverrides)
    {
      Environment.SetEnvironmentVariable(pair.Key, null);
    }
  }

  private void TryDeleteFileStorageRoot()
  {
    if (!Directory.Exists(fileStorageRootPath))
    {
      return;
    }

    try
    {
      Directory.Delete(fileStorageRootPath, recursive: true);
    }
    catch (IOException)
    {
      // Best-effort cleanup after host shutdown; orphaned files are preferable to failing teardown.
    }
    catch (UnauthorizedAccessException)
    {
    }
  }
}
