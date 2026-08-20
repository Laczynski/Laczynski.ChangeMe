using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChangeMe.Backend.IntegrationTests.Support;

internal static class IntegrationApiJson
{
  internal static readonly JsonSerializerOptions SerializerOptions = new()
  {
    PropertyNameCaseInsensitive = true,
    Converters = { new JsonStringEnumConverter() }
  };

  public static async Task<T?> ReadValueAsync<T>(HttpContent content, CancellationToken cancellationToken)
  {
    await using var stream = await content.ReadAsStreamAsync(cancellationToken);
    using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

    if (!document.RootElement.TryGetProperty("value", out var valueElement))
      return default;

    return valueElement.Deserialize<T>(SerializerOptions);
  }

  public static string? ReadErrorMessage(string responseBody)
  {
    using var document = JsonDocument.Parse(responseBody);
    if (document.RootElement.TryGetProperty("errors", out var errorsElement) &&
        errorsElement.ValueKind == JsonValueKind.Array &&
        errorsElement.GetArrayLength() > 0)
    {
      return errorsElement[0].GetString();
    }

    if (document.RootElement.TryGetProperty("value", out _))
      return null;

    return document.RootElement.GetRawText();
  }
}
