namespace Template.Backend.Infrastructure.Configurations;

public sealed class DatabaseOptions
{
  public const string SectionName = nameof(DatabaseOptions);

  public bool ApplyMigrationsOnStartup { get; set; }
}
