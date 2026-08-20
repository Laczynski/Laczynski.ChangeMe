using ChangeMe.Backend.Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UseCases.Notifications.Services;

public sealed class NotificationRetentionOptionsValidator : IValidateOptions<NotificationRetentionOptions>
{
  public ValidateOptionsResult Validate(string? name, NotificationRetentionOptions options)
  {
    var failures = new List<string>();

    if (options.ReadRetentionDays <= 0)
      failures.Add("NotificationRetentionOptions.ReadRetentionDays must be greater than zero.");

    if (options.UnreadRetentionDays <= 0)
      failures.Add("NotificationRetentionOptions.UnreadRetentionDays must be greater than zero.");

    if (options.AbsoluteRetentionDays <= 0)
      failures.Add("NotificationRetentionOptions.AbsoluteRetentionDays must be greater than zero.");

    if (options.ReadRetentionDays > options.UnreadRetentionDays)
    {
      failures.Add(
        "NotificationRetentionOptions.ReadRetentionDays must not exceed NotificationRetentionOptions.UnreadRetentionDays.");
    }

    if (options.UnreadRetentionDays > options.AbsoluteRetentionDays)
    {
      failures.Add(
        "NotificationRetentionOptions.UnreadRetentionDays must not exceed NotificationRetentionOptions.AbsoluteRetentionDays.");
    }

    if (!CronExpressionValidation.IsValid(options.CleanupCronExpression))
    {
      failures.Add(
        "NotificationRetentionOptions.CleanupCronExpression must be a valid five-part cron expression.");
    }

    return failures.Count == 0
      ? ValidateOptionsResult.Success
      : ValidateOptionsResult.Fail(failures);
  }
}
