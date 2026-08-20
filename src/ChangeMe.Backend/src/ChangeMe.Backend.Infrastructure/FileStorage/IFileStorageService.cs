namespace ChangeMe.Backend.Infrastructure.FileStorage;

public sealed record StoredFileKey(string Container, Guid OwnerId, string StorageKey);

public interface IFileStorageService
{
  Task<Result> SaveAsync(
    string container,
    Guid ownerId,
    string storageKey,
    Stream content,
    CancellationToken cancellationToken);

  Task<Result<Stream>> OpenReadStreamAsync(
    string container,
    Guid ownerId,
    string storageKey,
    CancellationToken cancellationToken);

  Task<Result> DeleteAsync(
    string container,
    Guid ownerId,
    string storageKey,
    CancellationToken cancellationToken);

  Task<Result> DeleteManyAsync(
    string container,
    Guid ownerId,
    IEnumerable<string> storageKeys,
    CancellationToken cancellationToken);

  Task<IReadOnlyList<StoredFileKey>> ListStoredFileKeysAsync(
    CancellationToken cancellationToken);
}
