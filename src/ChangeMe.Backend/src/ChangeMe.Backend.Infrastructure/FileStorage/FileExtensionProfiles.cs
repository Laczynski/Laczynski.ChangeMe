namespace ChangeMe.Backend.Infrastructure.FileStorage;

internal static class FileExtensionProfiles
{
  private static readonly Dictionary<string, FileExtensionProfile> Profiles =
    new(StringComparer.OrdinalIgnoreCase)
    {
      [".pdf"] = new(["pdf"], "application/pdf"),
      [".png"] = new(["png"], "image/png"),
      [".jpg"] = new(["jpg", "jpeg"], "image/jpeg"),
      [".jpeg"] = new(["jpg", "jpeg"], "image/jpeg"),
      [".gif"] = new(["gif"], "image/gif"),
      [".txt"] = new(["txt"], "text/plain", RequiresBinarySignature: false),
      [".csv"] = new(["csv"], "text/csv", RequiresBinarySignature: false),
      [".docx"] = new(
        ["docx"],
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"),
      [".xlsx"] = new(
        ["xlsx"],
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"),
    };

  public static IEnumerable<string> AllDetectableExtensions =>
    Profiles.Values.SelectMany(profile => profile.DetectableExtensions).Distinct(StringComparer.OrdinalIgnoreCase);

  public static bool IsConfiguredExtension(string extension) =>
    Profiles.ContainsKey(NormalizeExtension(extension));

  public static FileExtensionProfile? GetProfile(string extension) =>
    Profiles.TryGetValue(NormalizeExtension(extension), out var profile) ? profile : null;

  private static string NormalizeExtension(string extension) =>
    extension.StartsWith('.') ? extension.ToLowerInvariant() : $".{extension.ToLowerInvariant()}";
}

internal sealed record FileExtensionProfile(
  string[] DetectableExtensions,
  string ContentType,
  bool RequiresBinarySignature = true)
{
  public bool IsCompatibleWithDetectedExtensions(IEnumerable<string> detectedExtensions)
  {
    var normalizedDetected = detectedExtensions
      .Select(extension => extension.Trim().TrimStart('.').ToLowerInvariant())
      .ToHashSet(StringComparer.OrdinalIgnoreCase);

    return DetectableExtensions.Any(normalizedDetected.Contains);
  }
}
