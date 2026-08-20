namespace ChangeMe.Backend.Infrastructure.FileStorage;

public sealed class FileStorageOptions
{
  public const string SectionName = nameof(FileStorageOptions);

  public string RootPath { get; set; } = "../../storage";

  public string CleanupCronExpression { get; set; } = "0 * * * *";

  public int CleanupConcurrentExecutionTimeoutSeconds { get; set; } = 3600;
}
