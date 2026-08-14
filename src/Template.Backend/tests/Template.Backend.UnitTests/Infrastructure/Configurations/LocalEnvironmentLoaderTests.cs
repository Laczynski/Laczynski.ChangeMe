using Template.Backend.Infrastructure.Configurations;

namespace Template.Backend.UnitTests.Infrastructure.Configurations;

public sealed class LocalEnvironmentLoaderTests
{
  [Fact]
  public void LoadForDevelopment_LoadsParentEnvWithoutOverwritingProcessValues()
  {
    const string existingKey = "TEMPLATE_DOTENV_TEST_EXISTING";
    const string loadedKey = "TEMPLATE_DOTENV_TEST_LOADED";
    const string derivedKey = "TEMPLATE_DOTENV_TEST_DERIVED";
    var previousExistingValue = Environment.GetEnvironmentVariable(existingKey);
    var previousLoadedValue = Environment.GetEnvironmentVariable(loadedKey);
    var previousDerivedValue = Environment.GetEnvironmentVariable(derivedKey);
    var testRoot = Path.Combine(Path.GetTempPath(), "template-dotenv-tests", Guid.NewGuid().ToString("N"));
    var nestedDirectory = Path.Combine(testRoot, "src", "project");

    try
    {
      Directory.CreateDirectory(nestedDirectory);
      File.WriteAllText(
        Path.Combine(testRoot, ".env"),
        $"{existingKey}=from-file{Environment.NewLine}{loadedKey}=loaded-from-parent{Environment.NewLine}{derivedKey}=${{{existingKey}}}-derived{Environment.NewLine}");
      Environment.SetEnvironmentVariable(existingKey, "from-process");
      Environment.SetEnvironmentVariable(loadedKey, null);
      Environment.SetEnvironmentVariable(derivedKey, null);

      LocalEnvironmentLoader.LoadForDevelopment("Development", nestedDirectory);

      Assert.Equal("from-process", Environment.GetEnvironmentVariable(existingKey));
      Assert.Equal("loaded-from-parent", Environment.GetEnvironmentVariable(loadedKey));
      Assert.Equal("from-process-derived", Environment.GetEnvironmentVariable(derivedKey));
    }
    finally
    {
      Environment.SetEnvironmentVariable(existingKey, previousExistingValue);
      Environment.SetEnvironmentVariable(loadedKey, previousLoadedValue);
      Environment.SetEnvironmentVariable(derivedKey, previousDerivedValue);

      if (Directory.Exists(testRoot))
        Directory.Delete(testRoot, recursive: true);
    }
  }

  [Fact]
  public void LoadForDevelopment_AllowsMissingEnvFile()
  {
    var testRoot = Path.Combine(Path.GetTempPath(), "template-dotenv-tests", Guid.NewGuid().ToString("N"));

    try
    {
      Directory.CreateDirectory(testRoot);

      var exception = Record.Exception(
        () => LocalEnvironmentLoader.LoadForDevelopment("Development", testRoot));

      Assert.Null(exception);
    }
    finally
    {
      if (Directory.Exists(testRoot))
        Directory.Delete(testRoot, recursive: true);
    }
  }

  [Fact]
  public void LoadForDevelopment_DoesNotLoadEnvOutsideDevelopment()
  {
    const string key = "TEMPLATE_DOTENV_TEST_PRODUCTION";
    var previousValue = Environment.GetEnvironmentVariable(key);
    var testRoot = Path.Combine(Path.GetTempPath(), "template-dotenv-tests", Guid.NewGuid().ToString("N"));

    try
    {
      Directory.CreateDirectory(testRoot);
      File.WriteAllText(Path.Combine(testRoot, ".env"), $"{key}=from-file{Environment.NewLine}");
      Environment.SetEnvironmentVariable(key, null);

      LocalEnvironmentLoader.LoadForDevelopment("Production", testRoot);

      Assert.Null(Environment.GetEnvironmentVariable(key));
    }
    finally
    {
      Environment.SetEnvironmentVariable(key, previousValue);

      if (Directory.Exists(testRoot))
        Directory.Delete(testRoot, recursive: true);
    }
  }
}
