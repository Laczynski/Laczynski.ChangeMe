namespace ChangeMe.Backend.Infrastructure.FileStorage;

public interface IFileContentValidator
{
  Result<FileContentValidationResult> Validate(
    string originalFileName,
    string? declaredContentType,
    ReadOnlySpan<byte> contentPreview,
    long sizeBytes,
    int maxFileSizeBytes,
    IReadOnlyCollection<string> allowedExtensions);
}

public sealed record FileContentValidationResult(
  string SanitizedFileName,
  string ContentType,
  string Extension);
