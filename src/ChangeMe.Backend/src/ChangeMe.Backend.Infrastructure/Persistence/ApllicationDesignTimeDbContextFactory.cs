using ChangeMe.Backend.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Hosting;

namespace ChangeMe.Backend.Infrastructure.Persistence;

/// <summary>
/// Used by <c>dotnet ef migrations</c> when the startup project is not the API host.
/// </summary>
public sealed class ApplicationDesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
  public ApplicationDbContext CreateDbContext(string[] args)
  {
    var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
      ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
      ?? Environments.Development;
    LocalEnvironmentLoader.LoadForDevelopment(environmentName);

    var basePath = ResolveWebProjectPath();
    var configuration = new ConfigurationBuilder()
        .SetBasePath(basePath)
        .AddJsonFile("appsettings.json", optional: true)
        .AddJsonFile("appsettings.Development.json", optional: true)
        .AddEnvironmentVariables()
        .Build();

    var cs = ConnectionStringsOptionsValidator.GetValidatedDefaultConnection(configuration);

    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
    optionsBuilder.UseNpgsql(cs, npgsql =>
        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", DatabaseSchema.Default));

    return new ApplicationDbContext(optionsBuilder.Options);
  }

  private static string ResolveWebProjectPath()
  {
    var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

    while (currentDirectory is not null)
    {
      var candidate = Path.Combine(
          currentDirectory.FullName,
          "src",
          "ChangeMe.Backend",
          "src",
          "ChangeMe.Backend.Web");

      if (Directory.Exists(candidate))
      {
        return candidate;
      }

      currentDirectory = currentDirectory.Parent;
    }

    throw new InvalidOperationException(
        "Could not locate the ChangeMe.Backend.Web project directory for EF Core design-time.");
  }
}
