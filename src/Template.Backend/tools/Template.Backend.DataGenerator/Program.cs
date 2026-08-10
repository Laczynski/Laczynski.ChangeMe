using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Template.Backend.DataGenerator;
using Template.Backend.DataGenerator.Services;
using Template.Backend.Infrastructure.Configurations;
using Template.Backend.Infrastructure.Persistence;

var host = DataGeneratorHost.BuildHost(args);
var runOptions = host.Services.GetRequiredService<DataGeneratorRunOptions>();
var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("DataGenerator");

await using var scope = host.Services.CreateAsyncScope();
var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

logger.LogInformation("Applying migrations and system seed (roles, initial administrator)...");

await DatabaseConfig.InitializeDatabaseAsync(
  dbContext,
  configuration,
  passwordHasher,
  logger);

var existsChecker = scope.ServiceProvider.GetRequiredService<DemoDataExistsChecker>();
var hasDemoData = await existsChecker.HasDemoDataAsync(CancellationToken.None);

if (hasDemoData && !runOptions.Reset)
{
  logger.LogInformation("Demo data already exists. Use --reset to remove demo data and regenerate.");
  return;
}

if (runOptions.Reset && hasDemoData)
{
  logger.LogInformation("Removing existing demo data...");
  var cleaner = scope.ServiceProvider.GetRequiredService<DemoDataCleaner>();
  await cleaner.CleanAsync(CancellationToken.None);
}
else if (runOptions.Reset)
{
  logger.LogInformation("No demo data to remove.");
}

var orchestrator = scope.ServiceProvider.GetRequiredService<DemoDataGeneratorOrchestrator>();
await orchestrator.GenerateAsync(CancellationToken.None);

logger.LogInformation("Data generation completed.");
