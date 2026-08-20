using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ChangeMe.Backend.Domain.Aggregates.Issue.Enums;
using ChangeMe.Backend.Domain.Aggregates.Notifications;
using ChangeMe.Backend.Domain.Aggregates.Notifications.Enums;
using ChangeMe.Backend.Infrastructure.Persistence;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support;
using ChangeMe.Backend.IntegrationTests.Support.Fakes;
using ChangeMe.Backend.UseCases.Notifications.Services;
using DataGrid.Abstractions;
using DataGrid.Abstractions.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeMe.Backend.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class NotificationsEndpointTests(BackendWebApplicationFactory factory)
{
  [Fact]
  public async Task GetNotifications_AfterWatchedIssueIsUpdated_ShouldReturnUnreadNotificationAndAllowMarkAsRead()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var owner = await CreateAuthenticatedUserAsync(cancellationToken);
    var watcher = await CreateAuthenticatedUserAsync(cancellationToken);
    using var ownerClient = owner.Client;
    using var watcherClient = watcher.Client;

    await using (var clearScope = factory.Services.CreateAsyncScope())
    {
      var emailService = (FakeEmailService)clearScope.ServiceProvider.GetRequiredService<IEmailService>();
      emailService.Clear();
    }

    var createResponse = await ownerClient.PostAsJsonAsync("/api/v1/issues", new
    {
      Title = "Issue that will notify watchers",
      Description = "Issue description",
      Status = IssueStatus.NEW,
      Priority = IssuePriority.MEDIUM,
      AssignedToUserId = watcher.UserId,
      WatchAfterCreate = false
    }, cancellationToken);
    createResponse.EnsureSuccessStatusCode();

    var createBody = await createResponse.Content.ReadAsStringAsync(cancellationToken);
    var issueId = ExtractId(createBody);

    var watchResponse = await watcherClient.PostAsJsonAsync($"/api/v1/issues/{issueId}/watch", new
    {
      IssueId = issueId
    }, cancellationToken);
    watchResponse.EnsureSuccessStatusCode();

    var updateResponse = await ownerClient.PutAsJsonAsync($"/api/v1/issues/{issueId}", new
    {
      Id = issueId,
      Title = "Issue that will notify watchers",
      Description = "Issue description",
      Status = IssueStatus.IN_PROGRESS,
      Priority = IssuePriority.CRITICAL,
      AssignedToUserId = watcher.UserId,
      AcceptanceCriteria = Array.Empty<object>()
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

    var notificationsResponse = await watcherClient.GetAsync("/api/v1/notifications", cancellationToken);
    var notificationsBody = await notificationsResponse.Content.ReadAsStringAsync(cancellationToken);

    Assert.Equal(HttpStatusCode.OK, notificationsResponse.StatusCode);
    Assert.Contains("Issue that will notify watchers", notificationsBody, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("PRIORITY_CHANGED", notificationsBody, StringComparison.OrdinalIgnoreCase);

    var notificationId = ExtractFirstNotificationId(notificationsBody);

    var markAsReadResponse = await watcherClient.PutAsJsonAsync($"/api/v1/notifications/{notificationId}/read", new
    {
      NotificationId = notificationId
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.OK, markAsReadResponse.StatusCode);

    await using var scope = factory.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var notification = await dbContext.Notifications.FindAsync([notificationId], cancellationToken);

    Assert.NotNull(notification);
    Assert.Equal(watcher.UserId, notification.RecipientUserId);
    Assert.True(notification.IsRead);
    Assert.NotNull(notification.EmailSentAt);
  }

  [Fact]
  public async Task GetNotifications_AfterWatchedIssueAcceptanceCriteriaChange_ShouldReturnNotificationsAndSendEmail()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var owner = await CreateAuthenticatedUserAsync(cancellationToken);
    var watcher = await CreateAuthenticatedUserAsync(cancellationToken);
    using var ownerClient = owner.Client;
    using var watcherClient = watcher.Client;

    await using (var clearScope = factory.Services.CreateAsyncScope())
    {
      ((FakeEmailService)clearScope.ServiceProvider.GetRequiredService<IEmailService>()).Clear();
    }

    var createResponse = await ownerClient.PostAsJsonAsync("/api/v1/issues", new
    {
      Title = "Issue with acceptance criteria",
      Description = "Issue description",
      Status = IssueStatus.NEW,
      Priority = IssuePriority.MEDIUM,
      WatchAfterCreate = false,
      AcceptanceCriteria = new[]
      {
        new { Content = "Original criterion" },
        new { Content = "Criterion to remove" }
      }
    }, cancellationToken);
    createResponse.EnsureSuccessStatusCode();

    var createBody = await createResponse.Content.ReadAsStringAsync(cancellationToken);
    var issueId = ExtractId(createBody);

    await using var arrangeScope = factory.Services.CreateAsyncScope();
    var arrangeDb = arrangeScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var existingCriterionId = await arrangeDb.Issues
      .AsNoTracking()
      .Where(x => x.Id == issueId)
      .SelectMany(x => x.AcceptanceCriteria)
      .Where(x => x.Content == "Original criterion")
      .Select(x => x.Id)
      .SingleAsync(cancellationToken);

    var watchResponse = await watcherClient.PostAsJsonAsync($"/api/v1/issues/{issueId}/watch", new
    {
      IssueId = issueId
    }, cancellationToken);
    watchResponse.EnsureSuccessStatusCode();

    var updateResponse = await ownerClient.PutAsJsonAsync($"/api/v1/issues/{issueId}", new
    {
      Id = issueId,
      Title = "Issue with acceptance criteria",
      Description = "Issue description",
      Status = IssueStatus.NEW,
      Priority = IssuePriority.MEDIUM,
      AssignedToUserId = (Guid?)null,
      AcceptanceCriteria = new object[]
      {
        new
        {
          Id = existingCriterionId,
          Content = "Updated criterion"
        },
        new
        {
          Content = "Added criterion"
        }
      }
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

    var notificationsResponse = await watcherClient.GetAsync("/api/v1/notifications", cancellationToken);
    var notificationsBody = await notificationsResponse.Content.ReadAsStringAsync(cancellationToken);

    Assert.Equal(HttpStatusCode.OK, notificationsResponse.StatusCode);
    Assert.Contains("ACCEPTANCE_CRITERION_UPDATED", notificationsBody, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("ACCEPTANCE_CRITERION_ADDED", notificationsBody, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("ACCEPTANCE_CRITERION_REMOVED", notificationsBody, StringComparison.OrdinalIgnoreCase);

    await using var assertScope = factory.Services.CreateAsyncScope();
    var assertDb = assertScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var notifications = await assertDb.Notifications
      .AsNoTracking()
      .Where(n => n.RecipientUserId == watcher.UserId && n.IssueId == issueId)
      .ToListAsync(cancellationToken);

    Assert.Equal(3, notifications.Count);
    Assert.All(notifications, n => Assert.NotNull(n.EmailSentAt));

    var watcherEmail = await assertDb.Users
      .AsNoTracking()
      .Where(u => u.Id == watcher.UserId)
      .Select(u => u.Email)
      .SingleAsync(cancellationToken);

    var emailService = (FakeEmailService)assertScope.ServiceProvider.GetRequiredService<IEmailService>();
    Assert.Equal(3, emailService.SentEmails.Count(email => email.Recipients.Contains(watcherEmail)));
  }

  [Fact]
  public async Task GetNotifications_WithPagination_ShouldReturnDistinctPagesSortedNewestFirst()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await CreateAuthenticatedUserAsync(cancellationToken);
    using var client = user.Client;

    await using var scope = factory.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    var seededIds = new List<Guid>();
    for (var index = 0; index < 12; index++)
    {
      var notificationId = await SeedNotificationAsync(
        dbContext,
        user.UserId,
        $"Notification {index:D2}",
        NotificationEventType.STATUS_CHANGED,
        DateTime.UtcNow.AddMinutes(-index),
        isRead: false,
        readAt: null,
        cancellationToken);
      seededIds.Add(notificationId);
    }

    var pageOneGrid = GridQueryJson.Serialize(new GridQuery
    {
      Take = 10,
      Sort = [new SortDescriptor { Field = "CreatedAt", Desc = true }]
    });

    var pageOneResponse = await client.GetAsync(
      $"/api/v1/notifications?isRead=false&grid={Uri.EscapeDataString(pageOneGrid)}",
      cancellationToken);
    var pageOneBody = await pageOneResponse.Content.ReadAsStringAsync(cancellationToken);

    Assert.Equal(HttpStatusCode.OK, pageOneResponse.StatusCode);

    using var pageOneDocument = JsonDocument.Parse(pageOneBody);
    var pageOneValue = pageOneDocument.RootElement.GetProperty("value");
    var pageOneItems = pageOneValue.GetProperty("page").GetProperty("items").EnumerateArray().ToList();

    Assert.Equal(10, pageOneItems.Count);
    Assert.Equal(12, pageOneValue.GetProperty("page").GetProperty("totalCount").GetInt32());

    var pageOneIds = pageOneItems.Select(item => item.GetProperty("id").GetGuid()).ToList();
    Assert.Equal(seededIds.Take(10), pageOneIds);

    var pageOneCreatedAt = pageOneItems
      .Select(item => item.GetProperty("createdAt").GetDateTime())
      .ToList();
    Assert.True(
      pageOneCreatedAt.SequenceEqual(pageOneCreatedAt.OrderByDescending(timestamp => timestamp)));

    var pageTwoGrid = GridQueryJson.Serialize(new GridQuery
    {
      Skip = 10,
      Take = 10,
      Sort = [new SortDescriptor { Field = "CreatedAt", Desc = true }]
    });

    var pageTwoResponse = await client.GetAsync(
      $"/api/v1/notifications?isRead=false&grid={Uri.EscapeDataString(pageTwoGrid)}",
      cancellationToken);
    var pageTwoBody = await pageTwoResponse.Content.ReadAsStringAsync(cancellationToken);

    Assert.Equal(HttpStatusCode.OK, pageTwoResponse.StatusCode);

    using var pageTwoDocument = JsonDocument.Parse(pageTwoBody);
    var pageTwoItems = pageTwoDocument.RootElement
      .GetProperty("value")
      .GetProperty("page")
      .GetProperty("items")
      .EnumerateArray()
      .ToList();

    Assert.Equal(2, pageTwoItems.Count);

    var pageTwoIds = pageTwoItems.Select(item => item.GetProperty("id").GetGuid()).ToList();
    Assert.Equal(seededIds.Skip(10), pageTwoIds);
    Assert.Empty(pageOneIds.Intersect(pageTwoIds));
  }

  [Fact]
  public async Task GetNotifications_ShouldFilterOutExpiredNotificationsFromResponse()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await CreateAuthenticatedUserAsync(cancellationToken);
    using var client = user.Client;

    await using var scope = factory.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    await SeedNotificationAsync(
      dbContext,
      user.UserId,
      "Fresh notification",
      NotificationEventType.STATUS_CHANGED,
      DateTime.UtcNow.AddDays(-5),
      isRead: false,
      readAt: null,
      cancellationToken);

    await SeedNotificationAsync(
      dbContext,
      user.UserId,
      "Expired unread notification",
      NotificationEventType.PRIORITY_CHANGED,
      DateTime.UtcNow.AddDays(-91),
      isRead: false,
      readAt: null,
      cancellationToken);

    await SeedNotificationAsync(
      dbContext,
      user.UserId,
      "Expired read notification",
      NotificationEventType.COMMENT_CREATED,
      DateTime.UtcNow.AddDays(-40),
      isRead: true,
      readAt: DateTime.UtcNow.AddDays(-31),
      cancellationToken);

    var response = await client.GetAsync("/api/v1/notifications", cancellationToken);
    var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.Contains("Fresh notification", responseBody, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("Expired unread notification", responseBody, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("Expired read notification", responseBody, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public async Task NotificationRetentionCleanupJob_ShouldDeleteExpiredNotificationsAndKeepActiveOnes()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await CreateAuthenticatedUserAsync(cancellationToken);

    await using var scope = factory.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    var activeNotificationId = await SeedNotificationAsync(
      dbContext,
      user.UserId,
      "Active notification",
      NotificationEventType.STATUS_CHANGED,
      DateTime.UtcNow.AddDays(-2),
      isRead: false,
      readAt: null,
      cancellationToken);

    var expiredNotificationId = await SeedNotificationAsync(
      dbContext,
      user.UserId,
      "Expired absolute notification",
      NotificationEventType.ISSUE_CLOSED,
      DateTime.UtcNow.AddDays(-181),
      isRead: false,
      readAt: null,
      cancellationToken);

    var cleanupJob = scope.ServiceProvider.GetRequiredService<NotificationRetentionCleanupJob>();
    await cleanupJob.ExecuteAsync(cancellationToken);

    var remainingNotifications = await dbContext.Notifications
      .AsNoTracking()
      .Where(n => n.RecipientUserId == user.UserId)
      .Select(n => n.Id)
      .ToListAsync(cancellationToken);

    Assert.Contains(activeNotificationId, remainingNotifications);
    Assert.DoesNotContain(expiredNotificationId, remainingNotifications);
  }

  [Fact]
  public async Task MarkAllNotificationsAsRead_ShouldMarkUnreadNotificationsAsReadAndReturnSuccess()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await CreateAuthenticatedUserAsync(cancellationToken);
    using var client = user.Client;

    await using var scope = factory.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    var firstNotificationId = await SeedNotificationAsync(
      dbContext,
      user.UserId,
      "Unread notification 1",
      NotificationEventType.STATUS_CHANGED,
      DateTime.UtcNow.AddMinutes(-5),
      isRead: false,
      readAt: null,
      cancellationToken);

    var secondNotificationId = await SeedNotificationAsync(
      dbContext,
      user.UserId,
      "Unread notification 2",
      NotificationEventType.PRIORITY_CHANGED,
      DateTime.UtcNow.AddMinutes(-4),
      isRead: false,
      readAt: null,
      cancellationToken);

    var existingReadNotificationId = await SeedNotificationAsync(
      dbContext,
      user.UserId,
      "Already read notification",
      NotificationEventType.COMMENT_CREATED,
      DateTime.UtcNow.AddMinutes(-3),
      isRead: true,
      readAt: DateTime.UtcNow.AddMinutes(-2),
      cancellationToken);

    var response = await client.PutAsync("/api/v1/notifications/read-all", null, cancellationToken);

    var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    using var document = JsonDocument.Parse(responseBody);
    Assert.True(document.RootElement.GetProperty("value").GetBoolean());

    await using var assertScope = factory.Services.CreateAsyncScope();
    var assertDb = assertScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var notifications = await assertDb.Notifications
      .AsNoTracking()
      .Where(n => n.RecipientUserId == user.UserId)
      .ToDictionaryAsync(n => n.Id, cancellationToken);

    Assert.True(notifications[firstNotificationId].IsRead);
    Assert.True(notifications[secondNotificationId].IsRead);
    Assert.True(notifications[existingReadNotificationId].IsRead);
    Assert.NotNull(notifications[firstNotificationId].ReadAt);
    Assert.NotNull(notifications[secondNotificationId].ReadAt);
    Assert.NotNull(notifications[existingReadNotificationId].ReadAt);
  }

  private async Task<AuthenticatedNotificationUser> CreateAuthenticatedUserAsync(CancellationToken cancellationToken)
  {
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    return new AuthenticatedNotificationUser(user.Client, user.UserId);
  }

  private static Guid ExtractId(string responseBody)
  {
    using var document = JsonDocument.Parse(responseBody);
    return document.RootElement.GetProperty("value").GetProperty("id").GetGuid();
  }

  private static Guid ExtractFirstNotificationId(string responseBody)
  {
    using var document = JsonDocument.Parse(responseBody);
    return document.RootElement.GetProperty("value").GetProperty("page").GetProperty("items")[0]
      .GetProperty("id").GetGuid();
  }

  private static async Task<Guid> SeedNotificationAsync(
    ApplicationDbContext dbContext,
    Guid recipientUserId,
    string issueTitle,
    NotificationEventType eventType,
    DateTime createdAt,
    bool isRead,
    DateTime? readAt,
    CancellationToken cancellationToken)
  {
    var notificationResult = Notification.Create(
      recipientUserId,
      Guid.NewGuid(),
      Guid.NewGuid(),
      eventType,
      issueTitle,
      $"Message for {issueTitle}",
      $"/issues/{Guid.NewGuid()}");

    Assert.True(notificationResult.IsSuccess);

    var notification = notificationResult.Value;
    if (isRead)
      notification.MarkAsRead();

    dbContext.Notifications.Add(notification);
    dbContext.Entry(notification).Property(nameof(Notification.CreatedBy)).CurrentValue = recipientUserId;
    dbContext.Entry(notification).Property(nameof(Notification.UpdatedBy)).CurrentValue = recipientUserId;
    await dbContext.SaveChangesAsync(cancellationToken);

    dbContext.Entry(notification).Property(nameof(Notification.CreatedAt)).CurrentValue = createdAt;

    if (isRead)
      dbContext.Entry(notification).Property(nameof(Notification.ReadAt)).CurrentValue = readAt;

    await dbContext.SaveChangesAsync(cancellationToken);
    dbContext.ChangeTracker.Clear();

    return notification.Id;
  }

  private sealed record AuthenticatedNotificationUser(HttpClient Client, Guid UserId);
}
