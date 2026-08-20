using Serilog.Settings.Configuration;

namespace ChangeMe.Backend.Web.Configurations;

public static class SerilogOptions
{
  public const string SectionName = nameof(SerilogOptions);
}

public static class SerilogConfig
{
  public static WebApplicationBuilder AddSerilog(this WebApplicationBuilder builder)
  {
    builder.Logging.ClearProviders();

    builder.Host.UseSerilog(
        (context, configuration) => configuration.ReadFrom.Configuration(
          context.Configuration,
          new ConfigurationReaderOptions { SectionName = SerilogOptions.SectionName }));

    return builder;
  }
}
