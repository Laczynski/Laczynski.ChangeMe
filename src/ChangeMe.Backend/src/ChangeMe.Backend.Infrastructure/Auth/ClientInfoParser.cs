namespace ChangeMe.Backend.Infrastructure.Auth;

public static class ClientInfoParser
{
  public static string ParseDeviceBrowserLabel(string? userAgent)
  {
    if (string.IsNullOrWhiteSpace(userAgent))
      return "Unknown browser on Unknown";

    var browser = "Unknown browser";
    var platform = "Unknown";

    if (userAgent.Contains("Edg/", StringComparison.OrdinalIgnoreCase))
      browser = "Edge";
    else if (userAgent.Contains("Chrome/", StringComparison.OrdinalIgnoreCase))
      browser = "Chrome";
    else if (userAgent.Contains("Firefox/", StringComparison.OrdinalIgnoreCase))
      browser = "Firefox";
    else if (userAgent.Contains("Safari/", StringComparison.OrdinalIgnoreCase))
      browser = "Safari";

    if (userAgent.Contains("Windows", StringComparison.OrdinalIgnoreCase))
      platform = "Windows";
    else if (userAgent.Contains("Mac OS X", StringComparison.OrdinalIgnoreCase) ||
             userAgent.Contains("Macintosh", StringComparison.OrdinalIgnoreCase))
      platform = "macOS";
    else if (userAgent.Contains("Android", StringComparison.OrdinalIgnoreCase))
      platform = "Android";
    else if (userAgent.Contains("iPhone", StringComparison.OrdinalIgnoreCase) ||
             userAgent.Contains("iPad", StringComparison.OrdinalIgnoreCase))
      platform = "iOS";
    else if (userAgent.Contains("Linux", StringComparison.OrdinalIgnoreCase))
      platform = "Linux";

    return $"{browser} on {platform}";
  }
}
