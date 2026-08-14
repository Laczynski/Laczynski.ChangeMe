using Microsoft.Extensions.Options;

namespace Template.Backend.Infrastructure.Configurations;

public static class OptionsValidation
{
  public static IServiceCollection AddValidatedOptions<TOptions, TValidator>(
    this IServiceCollection services,
    IConfiguration configuration,
    string sectionName)
    where TOptions : class
    where TValidator : class, IValidateOptions<TOptions>
  {
    services.AddSingleton<IValidateOptions<TOptions>, TValidator>();
    services.AddOptions<TOptions>()
      .Bind(configuration.GetSection(sectionName))
      .ValidateOnStart();

    return services;
  }

  public static TOptions GetValidated<TOptions>(
    IConfiguration configuration,
    string sectionName,
    IValidateOptions<TOptions> validator)
    where TOptions : class, new()
  {
    var options = configuration.GetSection(sectionName).Get<TOptions>() ?? new TOptions();
    ThrowIfInvalid(validator, options, sectionName);
    return options;
  }

  private static void ThrowIfInvalid<TOptions>(
    IValidateOptions<TOptions> validator,
    TOptions options,
    string optionsName)
    where TOptions : class
  {
    var result = validator.Validate(optionsName, options);
    if (result.Failed)
      throw new OptionsValidationException(optionsName, typeof(TOptions), result.Failures);
  }
}
