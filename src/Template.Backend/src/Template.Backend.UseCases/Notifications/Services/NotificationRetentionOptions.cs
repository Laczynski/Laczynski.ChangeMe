namespace Template.Backend.UseCases.Notifications.Services;

public sealed class NotificationRetentionOptions
{
  public const string SectionName = nameof(NotificationRetentionOptions);

  public int UnreadRetentionDays { get; set; } = 90;
  public int ReadRetentionDays { get; set; } = 30;
  public int AbsoluteRetentionDays { get; set; } = 180;
  public string CleanupCronExpression { get; set; } = "0 3 * * *";
}
