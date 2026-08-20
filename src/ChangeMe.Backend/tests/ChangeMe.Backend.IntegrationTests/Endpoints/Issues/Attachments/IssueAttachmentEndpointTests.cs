using System.Net;
using System.Net.Http.Headers;
using System.Text;
using ChangeMe.Backend.Domain.Aggregates.Issue;
using ChangeMe.Backend.Domain.Aggregates.Issue.Enums;
using ChangeMe.Backend.Infrastructure.FileStorage;
using ChangeMe.Backend.Infrastructure.Persistence;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class IssueAttachmentEndpointTests(BackendWebApplicationFactory factory)
{
  [Fact]
  public async Task UploadIssueAttachment_WhenRequestIsValid_ShouldPersistAttachmentAndHistory()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    using var client = await TestAuthHelper.CreateAuthenticatedClientAsync(factory, cancellationToken);

    var issueId = await IssueTestHelper.SeedIssueAsync(
      factory,
      "Issue with attachments",
      "Issue description",
      IssuePriority.MEDIUM,
      null,
      cancellationToken);

    using var content = CreateTextFileContent("notes.txt", "Attachment content");

    var response = await client.PostAsync($"/api/v1/issues/{issueId}/attachments", content, cancellationToken);

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);

    await using var scope = factory.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var issue = await dbContext.Issues
      .Include(i => i.Attachments)
      .Include(i => i.HistoryEntries)
      .SingleAsync(i => i.Id == issueId, cancellationToken);

    Assert.Single(issue.Attachments);
    Assert.Equal("notes.txt", issue.Attachments.Single().OriginalFileName);
    Assert.Contains(
      issue.HistoryEntries,
      entry => entry.EventType == IssueHistoryEventType.ATTACHMENT_ADDED);
  }

  [Fact]
  public async Task UploadIssueAttachment_WhenContentDoesNotMatchExtension_ShouldReturnBadRequest()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    using var client = await TestAuthHelper.CreateAuthenticatedClientAsync(factory, cancellationToken);

    var issueId = await IssueTestHelper.SeedIssueAsync(
      factory,
      "Issue rejecting bad upload",
      "Issue description",
      IssuePriority.LOW,
      null,
      cancellationToken);

    using var content = CreateTextFileContent("report.pdf", "plain text content");

    var response = await client.PostAsync($"/api/v1/issues/{issueId}/attachments", content, cancellationToken);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task UploadIssueAttachment_WhenTextExtensionContainsPdfContent_ShouldReturnBadRequest()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    using var client = await TestAuthHelper.CreateAuthenticatedClientAsync(factory, cancellationToken);

    var issueId = await IssueTestHelper.SeedIssueAsync(
      factory,
      "Issue rejecting disguised upload",
      "Issue description",
      IssuePriority.LOW,
      null,
      cancellationToken);

    using var content = CreateFileContent(
      "notes.txt",
      "%PDF-1.7 disguised pdf content"u8.ToArray(),
      "text/plain");

    var response = await client.PostAsync($"/api/v1/issues/{issueId}/attachments", content, cancellationToken);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

    await using var scope = factory.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    Assert.False(await dbContext.IssueAttachments.AnyAsync(a => a.OwnerId == issueId, cancellationToken));
  }

  [Fact]
  public async Task DownloadIssueAttachment_WhenAttachmentExists_ShouldReturnFileContent()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    using var client = await TestAuthHelper.CreateAuthenticatedClientAsync(factory, cancellationToken);

    var issueId = await IssueTestHelper.SeedIssueAsync(
      factory,
      "Issue with downloadable attachment",
      "Issue description",
      IssuePriority.HIGH,
      null,
      cancellationToken);

    using (var uploadContent = CreateTextFileContent("download-me.txt", "download payload"))
    {
      var uploadResponse = await client.PostAsync(
        $"/api/v1/issues/{issueId}/attachments",
        uploadContent,
        cancellationToken);
      Assert.Equal(HttpStatusCode.Created, uploadResponse.StatusCode);
    }

    await using var scope = factory.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var attachmentId = await dbContext.IssueAttachments
      .Where(a => a.OwnerId == issueId)
      .Select(a => a.Id)
      .SingleAsync(cancellationToken);

    var downloadResponse = await client.GetAsync(
      $"/api/v1/issues/{issueId}/attachments/{attachmentId}/content",
      cancellationToken);

    Assert.Equal(HttpStatusCode.OK, downloadResponse.StatusCode);
    Assert.Equal("text/plain", downloadResponse.Content.Headers.ContentType?.MediaType);
    Assert.Contains("attachment", downloadResponse.Content.Headers.ContentDisposition?.DispositionType);
    Assert.Equal("download-me.txt", downloadResponse.Content.Headers.ContentDisposition?.FileName);
    Assert.True(downloadResponse.Headers.TryGetValues("X-Content-Type-Options", out var nosniffValues));
    Assert.Equal("nosniff", nosniffValues.Single());

    var downloadedText = await downloadResponse.Content.ReadAsStringAsync(cancellationToken);
    Assert.Equal("download payload", downloadedText);
  }

  [Fact]
  public async Task DeleteIssueAttachment_WhenUploaderDeletesAttachment_ShouldRemoveMetadataAndFile()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var authUser = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    using var client = authUser.Client;

    var issueId = await IssueTestHelper.SeedIssueAsync(
      factory,
      "Issue with removable attachment",
      "Issue description",
      IssuePriority.MEDIUM,
      null,
      cancellationToken,
      actorId: authUser.UserId);

    using (var uploadContent = CreateTextFileContent("remove-me.txt", "temporary content"))
    {
      var uploadResponse = await client.PostAsync(
        $"/api/v1/issues/{issueId}/attachments",
        uploadContent,
        cancellationToken);
      Assert.Equal(HttpStatusCode.Created, uploadResponse.StatusCode);
    }

    await using var scope = factory.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var attachment = await dbContext.IssueAttachments.SingleAsync(a => a.OwnerId == issueId, cancellationToken);

    var deleteResponse = await client.DeleteAsync(
      $"/api/v1/issues/{issueId}/attachments/{attachment.Id}",
      cancellationToken);

    Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

    Assert.False(await dbContext.IssueAttachments.AnyAsync(a => a.Id == attachment.Id, cancellationToken));
    Assert.Contains(
      await dbContext.IssueHistoryEntries.Where(h => h.IssueId == issueId).ToListAsync(cancellationToken),
      entry => entry.EventType == IssueHistoryEventType.ATTACHMENT_REMOVED);
  }

  [Fact]
  public async Task DeleteIssueAttachment_WhenDifferentUserDeletesAttachment_ShouldReturnForbidden()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var uploader = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    using var uploaderClient = uploader.Client;

    var issueId = await IssueTestHelper.SeedIssueAsync(
      factory,
      "Issue with protected attachment",
      "Issue description",
      IssuePriority.MEDIUM,
      null,
      cancellationToken,
      actorId: uploader.UserId);

    using (var uploadContent = CreateTextFileContent("protected.txt", "protected content"))
    {
      var uploadResponse = await uploaderClient.PostAsync(
        $"/api/v1/issues/{issueId}/attachments",
        uploadContent,
        cancellationToken);
      Assert.Equal(HttpStatusCode.Created, uploadResponse.StatusCode);
    }

    await using var scope = factory.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var attachmentId = await dbContext.IssueAttachments
      .Where(a => a.OwnerId == issueId)
      .Select(a => a.Id)
      .SingleAsync(cancellationToken);

    using var otherClient = await TestAuthHelper.CreateAuthenticatedClientAsync(factory, cancellationToken);
    var deleteResponse = await otherClient.DeleteAsync(
      $"/api/v1/issues/{issueId}/attachments/{attachmentId}",
      cancellationToken);

    Assert.Equal(HttpStatusCode.Forbidden, deleteResponse.StatusCode);
  }

  [Fact]
  public async Task UploadIssueAttachment_WhenIssueAlreadyHasMaxAttachments_ShouldReturnBadRequest()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    using var client = await TestAuthHelper.CreateAuthenticatedClientAsync(factory, cancellationToken);

    var issueId = await IssueTestHelper.SeedIssueAsync(
      factory,
      "Issue at attachment limit",
      "Issue description",
      IssuePriority.LOW,
      null,
      cancellationToken);

    for (var index = 0; index < IssueConstraints.ATTACHMENT_MAX_ATTACHMENTS_PER_ISSUE; index++)
    {
      using var content = CreateTextFileContent($"file-{index}.txt", $"content {index}");
      var response = await client.PostAsync($"/api/v1/issues/{issueId}/attachments", content, cancellationToken);
      Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    using (var overflowContent = CreateTextFileContent("overflow.txt", "one too many"))
    {
      var overflowResponse = await client.PostAsync(
        $"/api/v1/issues/{issueId}/attachments",
        overflowContent,
        cancellationToken);

      Assert.Equal(HttpStatusCode.BadRequest, overflowResponse.StatusCode);
    }

    await using var scope = factory.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    Assert.Equal(
      IssueConstraints.ATTACHMENT_MAX_ATTACHMENTS_PER_ISSUE,
      await dbContext.IssueAttachments.CountAsync(a => a.OwnerId == issueId, cancellationToken));
  }

  [Fact]
  public async Task DeleteIssue_WhenIssueHasAttachments_ShouldRemoveStoredFiles()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    using var client = await TestAuthHelper.CreateAuthenticatedClientAsync(factory, cancellationToken);

    var issueId = await IssueTestHelper.SeedIssueAsync(
      factory,
      "Issue with stored attachment",
      "Issue description",
      IssuePriority.MEDIUM,
      null,
      cancellationToken);

    using (var uploadContent = CreateTextFileContent("stored.txt", "stored payload"))
    {
      var uploadResponse = await client.PostAsync(
        $"/api/v1/issues/{issueId}/attachments",
        uploadContent,
        cancellationToken);
      Assert.Equal(HttpStatusCode.Created, uploadResponse.StatusCode);
    }

    await using var scope = factory.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var storageKey = await dbContext.IssueAttachments
      .Where(a => a.OwnerId == issueId)
      .Select(a => a.StorageKey)
      .SingleAsync(cancellationToken);

    var storedFilePath = GetStoredAttachmentPath(factory, issueId, storageKey);
    Assert.True(File.Exists(storedFilePath));

    var deleteIssueResponse = await client.DeleteAsync($"/api/v1/issues/{issueId}", cancellationToken);
    Assert.Equal(HttpStatusCode.OK, deleteIssueResponse.StatusCode);

    Assert.False(await dbContext.Issues.AnyAsync(i => i.Id == issueId, cancellationToken));
    Assert.False(File.Exists(storedFilePath));
    Assert.False(Directory.Exists(Path.GetDirectoryName(storedFilePath)));
  }

  [Fact]
  public async Task IssueAttachmentEndpoints_WhenUserIsAnonymous_ShouldReturnUnauthorized()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var issueId = Guid.CreateVersion7();
    var attachmentId = Guid.CreateVersion7();

    using var uploadContent = CreateTextFileContent("anonymous.txt", "anonymous upload");
    var uploadResponse = await client.PostAsync($"/api/v1/issues/{issueId}/attachments", uploadContent, cancellationToken);
    Assert.Equal(HttpStatusCode.Unauthorized, uploadResponse.StatusCode);

    var listResponse = await client.GetAsync($"/api/v1/issues/{issueId}/attachments", cancellationToken);
    Assert.Equal(HttpStatusCode.Unauthorized, listResponse.StatusCode);

    var downloadResponse = await client.GetAsync(
      $"/api/v1/issues/{issueId}/attachments/{attachmentId}/content",
      cancellationToken);
    Assert.Equal(HttpStatusCode.Unauthorized, downloadResponse.StatusCode);

    var deleteResponse = await client.DeleteAsync(
      $"/api/v1/issues/{issueId}/attachments/{attachmentId}",
      cancellationToken);
    Assert.Equal(HttpStatusCode.Unauthorized, deleteResponse.StatusCode);
  }

  private static string GetStoredAttachmentPath(
    BackendWebApplicationFactory factory,
    Guid issueId,
    string storageKey)
  {
    using var scope = factory.Services.CreateScope();
    var rootPath = scope.ServiceProvider.GetRequiredService<IOptions<FileStorageOptions>>().Value.RootPath;
    return Path.Combine(
      Path.GetFullPath(rootPath),
      IssueConstraints.STORAGE_CONTAINER,
      issueId.ToString("D"),
      storageKey);
  }

  private static MultipartFormDataContent CreateTextFileContent(string fileName, string text) =>
    CreateFileContent(fileName, Encoding.UTF8.GetBytes(text), "text/plain");

  private static MultipartFormDataContent CreateFileContent(string fileName, byte[] bytes, string contentType)
  {
    var content = new MultipartFormDataContent();
    var fileContent = new ByteArrayContent(bytes);
    fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
    content.Add(fileContent, "File", fileName);
    return content;
  }
}
