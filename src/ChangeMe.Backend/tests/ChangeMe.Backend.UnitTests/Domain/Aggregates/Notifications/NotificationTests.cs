using Ardalis.Result;
using ChangeMe.Backend.Domain.Aggregates.Notifications;
using ChangeMe.Backend.Domain.Aggregates.Notifications.Enums;

namespace ChangeMe.Backend.UnitTests;

public sealed class NotificationTests
{
  [Fact]
  public void Create_WhenInputIsValid_ShouldTrimStringsAndInitializeUnreadState()
  {
    var result = Notification.Create(
      Guid.CreateVersion7(),
      Guid.CreateVersion7(),
      Guid.CreateVersion7(),
      NotificationEventType.STATUS_CHANGED,
      "  Issue title  ",
      "  Notification message  ",
      "  /issues/123  ");

    Assert.True(result.IsSuccess);
    Assert.Equal("Issue title", result.Value.IssueTitle);
    Assert.Equal("Notification message", result.Value.Message);
    Assert.Equal("/issues/123", result.Value.Link);
    Assert.False(result.Value.IsRead);
    Assert.Null(result.Value.ReadAt);
    Assert.Null(result.Value.EmailSentAt);
  }

  [Fact]
  public void Create_WhenInputContainsMultipleValidationErrors_ShouldReturnInvalidResult()
  {
    var result = Notification.Create(
      Guid.Empty,
      Guid.Empty,
      Guid.Empty,
      (NotificationEventType)999,
      " ",
      " ",
      " ");

    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.Invalid, result.Status);
    Assert.Contains(result.ValidationErrors, error => error.Identifier == nameof(Notification.RecipientUserId));
    Assert.Contains(result.ValidationErrors, error => error.Identifier == nameof(Notification.IssueId));
    Assert.Contains(result.ValidationErrors, error => error.Identifier == nameof(Notification.IssueHistoryEntryId));
    Assert.Contains(result.ValidationErrors, error => error.Identifier == nameof(Notification.EventType));
    Assert.Contains(result.ValidationErrors, error => error.Identifier == nameof(Notification.IssueTitle));
    Assert.Contains(result.ValidationErrors, error => error.Identifier == nameof(Notification.Message));
    Assert.Contains(result.ValidationErrors, error => error.Identifier == nameof(Notification.Link));
  }

  [Theory]
  [InlineData(nameof(Notification.IssueTitle), 256, NotificationConstraints.ISSUE_TITLE_MAX_LENGTH)]
  [InlineData(nameof(Notification.Message), 1001, NotificationConstraints.MESSAGE_MAX_LENGTH)]
  [InlineData(nameof(Notification.Link), 501, NotificationConstraints.LINK_MAX_LENGTH)]
  public void Create_WhenStringFieldExceedsMaxLength_ShouldReturnInvalidResult(
    string fieldName,
    int actualLength,
    int expectedMaxLength)
  {
    var issueTitle = new string('T', fieldName == nameof(Notification.IssueTitle) ? actualLength : 10);
    var message = new string('M', fieldName == nameof(Notification.Message) ? actualLength : 10);
    var link = new string('L', fieldName == nameof(Notification.Link) ? actualLength : 20);

    var result = Notification.Create(
      Guid.CreateVersion7(),
      Guid.CreateVersion7(),
      Guid.CreateVersion7(),
      NotificationEventType.STATUS_CHANGED,
      issueTitle,
      message,
      link);

    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.Invalid, result.Status);
    Assert.Contains(result.ValidationErrors, error =>
      error.Identifier == fieldName
      && error.ErrorMessage.Contains(expectedMaxLength.ToString(), StringComparison.Ordinal));
  }

  [Fact]
  public void MarkAsRead_WhenCalledTwice_ShouldSetReadAtOnlyOnceAndKeepReadState()
  {
    var notification = Notification.Create(
      Guid.CreateVersion7(),
      Guid.CreateVersion7(),
      Guid.CreateVersion7(),
      NotificationEventType.STATUS_CHANGED,
      "Issue title",
      "Notification message",
      "/issues/123").Value;

    notification.MarkAsRead();
    var firstReadAt = notification.ReadAt;
    notification.MarkAsRead();

    Assert.True(notification.IsRead);
    Assert.NotNull(firstReadAt);
    Assert.Equal(firstReadAt, notification.ReadAt);
  }

  [Fact]
  public void MarkEmailSent_ShouldSetEmailSentAt()
  {
    var notification = Notification.Create(
      Guid.CreateVersion7(),
      Guid.CreateVersion7(),
      Guid.CreateVersion7(),
      NotificationEventType.STATUS_CHANGED,
      "Issue title",
      "Notification message",
      "/issues/123").Value;

    notification.MarkEmailSent();

    Assert.NotNull(notification.EmailSentAt);
  }
}
