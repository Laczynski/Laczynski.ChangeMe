namespace ChangeMe.Backend.DataGenerator.Options;

public sealed class DataGeneratorOptions
{
  public const string SectionName = nameof(DataGeneratorOptions);

  public int Seed { get; set; } = 20260522;
  public int Users { get; set; } = 8;
  public int Issues { get; set; } = 25;
  public int CommentsPerIssueMin { get; set; }
  public int CommentsPerIssueMax { get; set; } = 4;
  public int NotificationsPerUserMin { get; set; }
  public int NotificationsPerUserMax { get; set; } = 3;
  public string DefaultPassword { get; set; } = "Demo123!";
  public string EmailDomain { get; set; } = "demo.local";
}
