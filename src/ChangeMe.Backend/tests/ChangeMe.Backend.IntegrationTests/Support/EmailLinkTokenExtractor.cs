using System.Text.RegularExpressions;

namespace ChangeMe.Backend.IntegrationTests.Support;

internal static partial class EmailLinkTokenExtractor
{
  [GeneratedRegex(@"token=([^""&]+)")]
  private static partial Regex TokenRegex();

  public static string FromBody(string body)
  {
    var match = TokenRegex().Match(body);
    return match.Success ? Uri.UnescapeDataString(match.Groups[1].Value) : string.Empty;
  }
}
