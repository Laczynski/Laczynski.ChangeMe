using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.Infrastructure.FileStorage;

public sealed class LocalFileStorageService(IOptions<FileStorageOptions> options) : IFileStorageService
{
  public async Task<Result> SaveAsync(
    string container,
    Guid ownerId,
    string storageKey,
    Stream content,
    CancellationToken cancellationToken)
  {
    if (!TryGetFilePath(container, ownerId, storageKey, out var targetPath))
      return Result.Invalid(new ValidationError(nameof(storageKey), "storage key is invalid"));

    var ownerDirectory = Path.GetDirectoryName(targetPath)!;

    try
    {
      Directory.CreateDirectory(ownerDirectory);

      await using var targetStream = new FileStream(
        targetPath,
        new FileStreamOptions
        {
          Mode = FileMode.CreateNew,
          Access = FileAccess.Write,
          Share = FileShare.None,
          Options = FileOptions.Asynchronous,
        });

      await content.CopyToAsync(targetStream, cancellationToken);

      return Result.Success();
    }
    catch (IOException ex)
    {
      return Result.Error(ex.Message);
    }
    catch (UnauthorizedAccessException ex)
    {
      return Result.Error(ex.Message);
    }
  }

  public Task<Result<Stream>> OpenReadStreamAsync(
    string container,
    Guid ownerId,
    string storageKey,
    CancellationToken cancellationToken)
  {
    if (!TryGetFilePath(container, ownerId, storageKey, out var filePath))
      return Task.FromResult(Result<Stream>.Invalid(new ValidationError(nameof(storageKey), "storage key is invalid")));

    if (!File.Exists(filePath))
      return Task.FromResult(Result<Stream>.NotFound());

    Stream stream = new FileStream(
      filePath,
      new FileStreamOptions
      {
        Mode = FileMode.Open,
        Access = FileAccess.Read,
        Share = FileShare.Read | FileShare.Delete,
        Options = FileOptions.Asynchronous,
      });

    return Task.FromResult(Result.Success(stream));
  }

  public Task<Result> DeleteAsync(
    string container,
    Guid ownerId,
    string storageKey,
    CancellationToken cancellationToken)
  {
    if (!TryGetFilePath(container, ownerId, storageKey, out var filePath))
      return Task.FromResult(Result.Invalid(new ValidationError(nameof(storageKey), "storage key is invalid")));

    try
    {
      if (File.Exists(filePath))
        File.Delete(filePath);
    }
    catch (IOException ex)
    {
      return Task.FromResult(Result.Error(ex.Message));
    }

    TryDeleteEmptyOwnerDirectory(container, ownerId);

    return Task.FromResult(Result.Success());
  }

  public Task<Result> DeleteManyAsync(
    string container,
    Guid ownerId,
    IEnumerable<string> storageKeys,
    CancellationToken cancellationToken)
  {
    foreach (var storageKey in storageKeys)
    {
      if (!TryGetFilePath(container, ownerId, storageKey, out var filePath))
        continue;

      try
      {
        if (File.Exists(filePath))
          File.Delete(filePath);
      }
      catch (IOException ex)
      {
        return Task.FromResult(Result.Error(ex.Message));
      }
    }

    TryDeleteEmptyOwnerDirectory(container, ownerId);

    return Task.FromResult(Result.Success());
  }

  public Task<IReadOnlyList<StoredFileKey>> ListStoredFileKeysAsync(
    CancellationToken cancellationToken)
  {
    var storageRoot = Path.GetFullPath(options.Value.RootPath);
    if (!Directory.Exists(storageRoot))
      return Task.FromResult<IReadOnlyList<StoredFileKey>>([]);

    var storedKeys = new List<StoredFileKey>();

    foreach (var containerDirectory in Directory.EnumerateDirectories(storageRoot))
    {
      var container = Path.GetFileName(containerDirectory);
      if (string.IsNullOrWhiteSpace(container))
        continue;

      foreach (var ownerDirectory in Directory.EnumerateDirectories(containerDirectory))
      {
        if (!Guid.TryParse(Path.GetFileName(ownerDirectory), out var ownerId))
          continue;

        foreach (var filePath in Directory.EnumerateFiles(ownerDirectory))
        {
          cancellationToken.ThrowIfCancellationRequested();
          storedKeys.Add(new StoredFileKey(container, ownerId, Path.GetFileName(filePath)));
        }
      }
    }

    return Task.FromResult<IReadOnlyList<StoredFileKey>>(storedKeys);
  }

  private void TryDeleteEmptyOwnerDirectory(string container, Guid ownerId)
  {
    var ownerDirectory = GetOwnerDirectory(container, ownerId);
    if (Directory.Exists(ownerDirectory) && !Directory.EnumerateFileSystemEntries(ownerDirectory).Any())
      Directory.Delete(ownerDirectory);
  }

  private string GetOwnerDirectory(string container, Guid ownerId)
  {
    var rootPath = Path.GetFullPath(options.Value.RootPath);
    return Path.Combine(rootPath, container, ownerId.ToString("D"));
  }

  private bool TryGetFilePath(string container, Guid ownerId, string storageKey, out string filePath)
  {
    filePath = string.Empty;

    if (string.IsNullOrWhiteSpace(container)
        || container.Contains('/', StringComparison.Ordinal)
        || container.Contains('\\', StringComparison.Ordinal)
        || container.Contains("..", StringComparison.Ordinal))
      return false;

    if (string.IsNullOrWhiteSpace(storageKey)
        || storageKey.Contains('/', StringComparison.Ordinal)
        || storageKey.Contains('\\', StringComparison.Ordinal)
        || storageKey.Contains("..", StringComparison.Ordinal))
      return false;

    var ownerDirectory = GetOwnerDirectory(container, ownerId);
    filePath = Path.GetFullPath(Path.Combine(ownerDirectory, storageKey));

    if (!filePath.StartsWith(ownerDirectory, StringComparison.OrdinalIgnoreCase))
      return false;

    return true;
  }
}
