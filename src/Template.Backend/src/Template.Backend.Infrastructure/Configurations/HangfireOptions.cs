namespace Template.Backend.Infrastructure.Configurations;

public sealed class HangfireOptions
{
  public const string SectionName = nameof(HangfireOptions);

  public string DashboardPath { get; set; } = "/hangfire";

  public bool DashboardEnabled { get; set; }

  public bool ServerEnabled { get; set; } = true;
}
