namespace ChangeMe.Backend.Web.Configurations;

public sealed class RateLimitingOptions
{
  public const string SectionName = nameof(RateLimitingOptions);

  public bool Enabled { get; set; } = true;

  public int AuthPermitLimit { get; set; } = 10;

  public int AuthWindowSeconds { get; set; } = 60;

  public int ApiPermitLimit { get; set; } = 100;

  public int ApiWindowSeconds { get; set; } = 60;
}
