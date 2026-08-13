namespace Template.Backend.Web.Configurations;

public sealed record ApplicationStartupOptions(bool MigrateOnly, string[] ConfigurationArguments)
{
  public const string MigrateOnlyArgument = "--migrate-only";

  public static ApplicationStartupOptions Parse(IEnumerable<string> arguments)
  {
    var configurationArguments = new List<string>();
    var migrateOnly = false;

    foreach (var argument in arguments)
    {
      if (string.Equals(argument, MigrateOnlyArgument, StringComparison.Ordinal))
      {
        migrateOnly = true;
        continue;
      }

      configurationArguments.Add(argument);
    }

    return new ApplicationStartupOptions(migrateOnly, [.. configurationArguments]);
  }
}
