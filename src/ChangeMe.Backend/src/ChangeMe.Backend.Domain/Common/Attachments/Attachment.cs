namespace ChangeMe.Backend.Domain.Common.Attachments;

public abstract class Attachment : Entity
{
  public AttachmentType Type { get; protected set; }

  public string StorageContainer { get; protected set; } = string.Empty;
  public Guid OwnerId { get; protected set; }
  public string OriginalFileName { get; protected set; } = string.Empty;
  public string ContentType { get; protected set; } = string.Empty;
  public long SizeBytes { get; protected set; }
  public string StorageKey { get; protected set; } = string.Empty;

  protected static string GenerateStorageKey() => Guid.CreateVersion7().ToString("N");

  protected static List<ValidationError> ValidateMetadata(
    string storageContainer,
    Guid ownerId,
    string originalFileName,
    string contentType,
    long sizeBytes,
    int maxFileSizeBytes)
  {
    var validationErrors = new List<ValidationError>();

    if (string.IsNullOrWhiteSpace(storageContainer))
      validationErrors.Add(new ValidationError(nameof(StorageContainer), "cannot be empty"));
    else if (storageContainer.Trim().Length > AttachmentConstraints.STORAGE_CONTAINER_MAX_LENGTH)
      validationErrors.Add(new ValidationError(nameof(StorageContainer), $"cannot be longer than {AttachmentConstraints.STORAGE_CONTAINER_MAX_LENGTH} characters"));

    if (ownerId == Guid.Empty)
      validationErrors.Add(new ValidationError(nameof(OwnerId), "cannot be empty"));

    if (string.IsNullOrWhiteSpace(originalFileName))
      validationErrors.Add(new ValidationError(nameof(OriginalFileName), "cannot be empty"));
    else if (originalFileName.Trim().Length > AttachmentConstraints.ORIGINAL_FILE_NAME_MAX_LENGTH)
      validationErrors.Add(new ValidationError(nameof(OriginalFileName), $"cannot be longer than {AttachmentConstraints.ORIGINAL_FILE_NAME_MAX_LENGTH} characters"));

    if (string.IsNullOrWhiteSpace(contentType))
      validationErrors.Add(new ValidationError(nameof(ContentType), "cannot be empty"));
    else if (contentType.Trim().Length > AttachmentConstraints.CONTENT_TYPE_MAX_LENGTH)
      validationErrors.Add(new ValidationError(nameof(ContentType), $"cannot be longer than {AttachmentConstraints.CONTENT_TYPE_MAX_LENGTH} characters"));

    if (sizeBytes <= 0)
      validationErrors.Add(new ValidationError(nameof(SizeBytes), "must be greater than zero"));
    else if (sizeBytes > maxFileSizeBytes)
      validationErrors.Add(new ValidationError(nameof(SizeBytes), $"cannot exceed {maxFileSizeBytes} bytes"));

    return validationErrors;
  }
}
