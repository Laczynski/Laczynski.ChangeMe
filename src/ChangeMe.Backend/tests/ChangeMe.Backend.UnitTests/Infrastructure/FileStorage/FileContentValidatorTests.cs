using ChangeMe.Backend.Domain.Aggregates.Issue;
using ChangeMe.Backend.Domain.Common.Attachments;
using ChangeMe.Backend.Infrastructure.FileStorage;

namespace ChangeMe.Backend.UnitTests.Infrastructure.FileStorage;

public sealed class FileContentValidatorTests
{
  private readonly FileContentValidator validator = new(new FileContentInspectorProvider());

  [Fact]
  public void Validate_WhenPdfHasValidMagicBytes_ShouldSucceed()
  {
    var content = "%PDF-1.7 sample"u8.ToArray();

    var result = validator.Validate(
      "report.pdf",
      "application/pdf",
      content,
      content.LongLength,
      IssueConstraints.ATTACHMENT_MAX_FILE_SIZE_BYTES,
      IssueConstraints.ATTACHMENT_ALLOWED_EXTENSIONS);

    Assert.True(result.IsSuccess);
    Assert.Equal("application/pdf", result.Value.ContentType);
    Assert.Equal("report.pdf", result.Value.SanitizedFileName);
  }

  [Fact]
  public void Validate_WhenPdfExtensionHasTextContent_ShouldFail()
  {
    var content = "plain text"u8.ToArray();

    var result = validator.Validate(
      "report.pdf",
      "application/pdf",
      content,
      content.LongLength,
      IssueConstraints.ATTACHMENT_MAX_FILE_SIZE_BYTES,
      IssueConstraints.ATTACHMENT_ALLOWED_EXTENSIONS);

    Assert.False(result.IsSuccess);
  }

  [Fact]
  public void Validate_WhenFileExceedsConfiguredLimit_ShouldFail()
  {
    var result = validator.Validate(
      "notes.txt",
      "text/plain",
      "hello"u8.ToArray(),
      IssueConstraints.ATTACHMENT_MAX_FILE_SIZE_BYTES + 1,
      IssueConstraints.ATTACHMENT_MAX_FILE_SIZE_BYTES,
      IssueConstraints.ATTACHMENT_ALLOWED_EXTENSIONS);

    Assert.False(result.IsSuccess);
  }

  [Fact]
  public void Validate_WhenFileNameContainsPathSegments_ShouldSanitizeToLeafName()
  {
    var content = "hello"u8.ToArray();

    var result = validator.Validate(
      "../../notes.txt",
      "text/plain",
      content,
      content.LongLength,
      IssueConstraints.ATTACHMENT_MAX_FILE_SIZE_BYTES,
      IssueConstraints.ATTACHMENT_ALLOWED_EXTENSIONS);

    Assert.True(result.IsSuccess);
    Assert.Equal("notes.txt", result.Value.SanitizedFileName);
  }

  [Fact]
  public void Validate_WhenTxtExtensionContainsPdfContent_ShouldFail()
  {
    var content = "%PDF-1.7 sample"u8.ToArray();

    var result = validator.Validate(
      "notes.txt",
      "text/plain",
      content,
      content.LongLength,
      IssueConstraints.ATTACHMENT_MAX_FILE_SIZE_BYTES,
      IssueConstraints.ATTACHMENT_ALLOWED_EXTENSIONS);

    Assert.False(result.IsSuccess);
  }

  [Fact]
  public void Validate_WhenFileNameExceedsMaxLength_ShouldPreserveExtensionForContentValidation()
  {
    var content = "%PDF-1.7 sample"u8.ToArray();
    var fileName = $"{new string('a', 300)}.pdf";

    var result = validator.Validate(
      fileName,
      "application/pdf",
      content,
      content.LongLength,
      IssueConstraints.ATTACHMENT_MAX_FILE_SIZE_BYTES,
      IssueConstraints.ATTACHMENT_ALLOWED_EXTENSIONS);

    Assert.True(result.IsSuccess);
    Assert.Equal(".pdf", Path.GetExtension(result.Value.SanitizedFileName));
    Assert.True(result.Value.SanitizedFileName.Length <= AttachmentConstraints.ORIGINAL_FILE_NAME_MAX_LENGTH);
    Assert.Equal("application/pdf", result.Value.ContentType);
  }

  [Fact]
  public void Validate_WhenDeclaredContentTypeDoesNotMatchProfile_ShouldFail()
  {
    var content = "%PDF-1.7 sample"u8.ToArray();

    var result = validator.Validate(
      "report.pdf",
      "text/plain",
      content,
      content.LongLength,
      IssueConstraints.ATTACHMENT_MAX_FILE_SIZE_BYTES,
      IssueConstraints.ATTACHMENT_ALLOWED_EXTENSIONS);

    Assert.False(result.IsSuccess);
  }
}
