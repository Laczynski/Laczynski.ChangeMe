using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Template.Backend.Infrastructure.Persistence;

namespace Template.Backend.Infrastructure.Configurations;

public sealed class DatabaseOptions
{
  public const string SectionName = nameof(DatabaseOptions);

  public bool ApplyMigrationsOnStartup { get; set; }
}

public static class DatabaseConfig
{
  public static IServiceCollection AddDatabase(this IServiceCollection services, WebApplicationBuilder builder, ILogger logger)
    => AddDatabase(services, builder.Configuration, builder.Environment, logger, configureHealthChecks: true);

  public static IServiceCollection AddDatabase(
    this IServiceCollection services,
    IConfiguration configuration,
    IHostEnvironment environment,
    ILogger logger,
    bool configureHealthChecks = false)
  {
    services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));

    var connectionString = ConnectionStringsOptionsValidator.GetValidatedDefaultConnection(configuration);

    logger.LogInformation("Using PostgreSQL database");

    services.AddDbContext<ApplicationDbContext>(options =>
    {
      options.UseNpgsql(connectionString, npgsql =>
          npgsql.MigrationsHistoryTable("__EFMigrationsHistory", DatabaseSchema.Default));

      if (environment.IsDevelopment())
      {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
        logger.LogInformation("Sensitive data logging enabled for development");
      }
    });

    if (configureHealthChecks)
    {
      services.AddHealthChecks()
          .AddNpgSql(connectionString, name: "postgres", tags: ["db", "ready"]);
    }

    logger.LogInformation("PostgreSQL database connection configured");
    return services;
  }

  public static async Task UseDatabase(this WebApplication app)
  {
    var databaseOptions = app.Services.GetRequiredService<IOptions<DatabaseOptions>>().Value;
    if (!databaseOptions.ApplyMigrationsOnStartup)
      return;

    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await InitializeDatabaseAsync(
      db,
      scope.ServiceProvider.GetRequiredService<IConfiguration>(),
      scope.ServiceProvider.GetRequiredService<IPasswordHasher>(),
      scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>(),
      CancellationToken.None);
  }

  public static async Task InitializeDatabaseAsync(
    ApplicationDbContext dbContext,
    IConfiguration configuration,
    IPasswordHasher passwordHasher,
    ILogger logger,
    CancellationToken cancellationToken = default)
  {
    await ApplyPendingMigrationsAsync(dbContext, cancellationToken);
    await ApplicationDataSeeder.SeedAsync(dbContext, configuration, passwordHasher, logger, cancellationToken);
  }

  public static async Task ApplyPendingMigrationsAsync(
    ApplicationDbContext dbContext,
    CancellationToken cancellationToken = default)
  {
    if (!dbContext.Database.GetMigrations().Any())
    {
      throw new InvalidOperationException(
        """
        No EF Core migrations were found in the Infrastructure assembly. MigrateAsync() succeeds with zero migrations but creates no tables.
        Add an initial migration from the solution root (see docs/modules/backend/operations/persistence.md):
          npm run ef:migrations:add -- InitialCreate
        Then restart the API in Development (DatabaseOptions:ApplyMigrationsOnStartup is true in appsettings.Development.json), or run npm run ef:database:update.
        """);
    }

    await dbContext.Database.MigrateAsync(cancellationToken);
  }

}
