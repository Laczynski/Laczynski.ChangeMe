namespace ChangeMe.Backend.Web.Configurations;

public sealed class CorsOptions
{
  public const string SectionName = nameof(CorsOptions);

  public string[] AllowedOrigins { get; set; } = [];
}
