using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Template.Backend.DataGenerator.Generators;
using Template.Backend.DataGenerator.Options;
using Template.Backend.DataGenerator.Services;
using Template.Backend.Infrastructure.Auth;
using Template.Backend.Infrastructure.Configurations;

namespace Template.Backend.DataGenerator;

internal static class DataGeneratorHost
{
  public static IHost BuildHost(string[] args)
  {
    LocalEnvironmentLoader.LoadForDevelopment(Environments.Development);

    var reset = args.Contains("--reset", StringComparer.OrdinalIgnoreCase);
    var contentRoot = AppContext.BaseDirectory;

    var configuration = BuildConfiguration(contentRoot, args);

    var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
    {
      Args = args,
      ContentRootPath = contentRoot,
      EnvironmentName = Environments.Development,
      Configuration = configuration,
    });

    builder.Services.Configure<DataGeneratorOptions>(
      builder.Configuration.GetSection(DataGeneratorOptions.SectionName));

    var loggerFactory = LoggerFactory.Create(lb => lb.AddSimpleConsole(o => o.SingleLine = true));
    var logger = loggerFactory.CreateLogger("DataGenerator");

    builder.Services.AddSingleton<IPasswordHasher, PasswordHasherAdapter>();
    builder.Services.AddDatabase(builder.Configuration, builder.Environment, logger, configureHealthChecks: false);
    builder.Services.AddScoped<DemoDataExistsChecker>();
    builder.Services.AddScoped<DemoDataCleaner>();
    builder.Services.AddScoped<UsersGenerator>();
    builder.Services.AddScoped<IssuesGenerator>();
    builder.Services.AddScoped<NotificationsGenerator>();
    builder.Services.AddScoped<DemoDataGeneratorOrchestrator>();
    builder.Services.AddSingleton(new DataGeneratorRunOptions(reset));

    return builder.Build();
  }

  private static ConfigurationManager BuildConfiguration(string contentRoot, string[] args)
  {
    var appSettingsPath = Path.Combine(contentRoot, "appsettings.json");
    var developmentSettingsPath = Path.Combine(contentRoot, "appsettings.Development.json");

    if (!File.Exists(appSettingsPath))
    {
      throw new InvalidOperationException(
        $"""
        Missing {appSettingsPath}.
        Rebuild the DataGenerator project so Web appsettings are copied to the output directory:
          dotnet build src/Template.Backend/tools/Template.Backend.DataGenerator/Template.Backend.DataGenerator.csproj
        """);
    }

    if (!File.Exists(developmentSettingsPath))
    {
      throw new InvalidOperationException(
        $"""
        Missing {developmentSettingsPath}.
        Rebuild the DataGenerator project so Web appsettings.Development.json is copied to the output directory.
        """);
    }

    var configuration = new ConfigurationManager();
    configuration.SetBasePath(contentRoot);
    configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
    configuration.AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: false);
    configuration.AddEnvironmentVariables();
    if (args.Length > 0)
      configuration.AddCommandLine(args);

    _ = ConnectionStringsOptionsValidator.GetValidatedDefaultConnection(configuration);

    return configuration;
  }
}

internal sealed record DataGeneratorRunOptions(bool Reset);
