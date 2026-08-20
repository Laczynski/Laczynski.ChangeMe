using Cronos;

namespace ChangeMe.Backend.Infrastructure.Configurations;

public static class CronExpressionValidation
{
  public static bool IsValid(string? expression)
  {
    if (string.IsNullOrWhiteSpace(expression))
      return false;

    try
    {
      _ = CronExpression.Parse(expression, CronFormat.Standard);
      return true;
    }
    catch (CronFormatException)
    {
      return false;
    }
  }
}
