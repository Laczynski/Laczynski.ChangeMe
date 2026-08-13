using Microsoft.Extensions.Options;

namespace Template.Backend.Infrastructure.Configurations;

public static class OptionsValidation
{
  public static void ThrowIfInvalid<TOptions>(
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
