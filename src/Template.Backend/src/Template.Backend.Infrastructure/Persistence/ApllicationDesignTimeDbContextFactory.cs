using Microsoft.EntityFrameworkCore.Design;

namespace Template.Backend.Infrastructure.Persistence;

/// <summary>
/// Used by <c>dotnet ef migrations</c> when the startup project is not the API host.
/// </summary>
public sealed class ApplicationDesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
  public ApplicationDbContext CreateDbContext(string[] args)
  {
    var basePath = ResolveWebProjectPath();
    var configuration = new ConfigurationBuilder()
        .SetBasePath(basePath)
        .AddJsonFile("appsettings.json", optional: true)
        .AddJsonFile("appsettings.Development.json", optional: true)
        .AddEnvironmentVariables()
        .Build();

    var cs = configuration.GetConnectionString("DefaultConnection")
        ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
        ?? throw new InvalidOperationException(
            "Set ConnectionStrings:DefaultConnection in appsettings or ConnectionStrings__DefaultConnection for EF Core design-time.");

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
          "Template.Backend",
          "src",
          "Template.Backend.Web");

      if (Directory.Exists(candidate))
      {
        return candidate;
      }

      currentDirectory = currentDirectory.Parent;
    }

    throw new InvalidOperationException(
        "Could not locate the Template.Backend.Web project directory for EF Core design-time.");
  }
}
