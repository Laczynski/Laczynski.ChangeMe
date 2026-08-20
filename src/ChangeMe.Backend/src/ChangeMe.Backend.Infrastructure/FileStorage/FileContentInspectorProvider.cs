using System.Collections.Immutable;
using MimeDetective;
using MimeDetective.Storage;

namespace ChangeMe.Backend.Infrastructure.FileStorage;

public sealed class FileContentInspectorProvider
{
  private static readonly ImmutableHashSet<string> AllowedDetectableExtensions =
    FileExtensionProfiles.AllDetectableExtensions.ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);

  public IContentInspector Inspector { get; } = BuildInspector();

  private static IContentInspector BuildInspector()
  {
    var definitions = MimeDetective.Definitions.DefaultDefinitions.All()
      .ScopeExtensions(AllowedDetectableExtensions)
      .ToImmutableArray();

    return new ContentInspectorBuilder
    {
      Definitions = definitions,
      Parallel = false,
    }.Build();
  }
}
