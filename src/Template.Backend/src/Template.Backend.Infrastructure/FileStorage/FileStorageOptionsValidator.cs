using Microsoft.Extensions.Options;
using Template.Backend.Infrastructure.Configurations;

namespace Template.Backend.Infrastructure.FileStorage;

public sealed class FileStorageOptionsValidator : IValidateOptions<FileStorageOptions>
{
  public ValidateOptionsResult Validate(string? name, FileStorageOptions options)
  {
    var failures = new List<string>();

    if (string.IsNullOrWhiteSpace(options.RootPath))
      failures.Add("FileStorageOptions.RootPath is required.");

    if (!CronExpressionValidation.IsValid(options.CleanupCronExpression))
      failures.Add("FileStorageOptions.CleanupCronExpression must be a valid five-part cron expression.");

    if (options.CleanupConcurrentExecutionTimeoutSeconds <= 0)
    {
      failures.Add(
        "FileStorageOptions.CleanupConcurrentExecutionTimeoutSeconds must be greater than zero.");
    }

    return failures.Count == 0
      ? ValidateOptionsResult.Success
      : ValidateOptionsResult.Fail(failures);
  }
}
