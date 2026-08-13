using DotNetEnv;
using Microsoft.Extensions.Hosting;

namespace Template.Backend.Infrastructure.Configurations;

public static class LocalEnvironmentLoader
{
  public static void LoadForDevelopment(string[] args)
  {
    LoadForDevelopment(ResolveEnvironmentName(args), Directory.GetCurrentDirectory());
  }

  public static void LoadForDevelopment(string environmentName, string? startDirectory = null)
  {
    if (!string.Equals(environmentName, Environments.Development, StringComparison.OrdinalIgnoreCase))
      return;

    var searchDirectory = startDirectory ?? Directory.GetCurrentDirectory();
    var searchPath = Path.Combine(searchDirectory, ".env");

    try
    {
      Env.NoClobber().TraversePath().Load(searchPath);
    }
    catch (FileNotFoundException)
    {
      // Missing local configuration is reported by section-specific startup validation.
    }
  }

  private static string ResolveEnvironmentName(string[] args)
  {
    for (var index = 0; index < args.Length; index++)
    {
      var argument = args[index];
      if (argument.StartsWith("--environment=", StringComparison.OrdinalIgnoreCase))
        return argument["--environment=".Length..];

      if (string.Equals(argument, "--environment", StringComparison.OrdinalIgnoreCase) && index + 1 < args.Length)
        return args[index + 1];
    }

    return Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
      ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
      ?? Environments.Production;
  }
}
