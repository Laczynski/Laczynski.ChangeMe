using Template.Backend.Web.Configurations;

namespace Template.Backend.IntegrationTests.Configuration;

public sealed class ApplicationStartupOptionsTests
{
  [Fact]
  public void Parse_WithMigrateOnly_RemovesOperationalArgumentFromConfigurationArguments()
  {
    string[] arguments =
    [
      "--environment",
      "Production",
      ApplicationStartupOptions.MigrateOnlyArgument,
      "--AuthOptions:Jwt:Issuer=Example"
    ];

    var result = ApplicationStartupOptions.Parse(arguments);

    Assert.True(result.MigrateOnly);
    Assert.Equal(
      ["--environment", "Production", "--AuthOptions:Jwt:Issuer=Example"],
      result.ConfigurationArguments);
  }

  [Fact]
  public void Parse_WithoutMigrateOnly_PreservesConfigurationArguments()
  {
    string[] arguments = ["--environment", "Development"];

    var result = ApplicationStartupOptions.Parse(arguments);

    Assert.False(result.MigrateOnly);
    Assert.Equal(arguments, result.ConfigurationArguments);
  }
}
