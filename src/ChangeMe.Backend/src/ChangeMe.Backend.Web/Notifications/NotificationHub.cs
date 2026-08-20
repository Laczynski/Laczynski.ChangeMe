using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace ChangeMe.Backend.Web.Notifications;

public class NotificationHub : Hub
{
  public override async Task OnConnectedAsync()
  {
    var userIdValue = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    if (Guid.TryParse(userIdValue, out var userId))
      await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));

    await base.OnConnectedAsync();
  }

  public override async Task OnDisconnectedAsync(Exception? exception)
  {
    var userIdValue = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    if (Guid.TryParse(userIdValue, out var userId))
      await Groups.RemoveFromGroupAsync(Context.ConnectionId, UserGroup(userId));

    await base.OnDisconnectedAsync(exception);
  }

  public static string UserGroup(Guid userId) => $"notifications:{userId}";
}

public class SignalRNotificationRealtimePublisher(IHubContext<NotificationHub> hubContext) : INotificationRealtimePublisher
{
  public Task PublishAsync(Guid userId, NotificationRealtimeMessage message, CancellationToken cancellationToken)
  {
    return hubContext.Clients.Group(NotificationHub.UserGroup(userId))
      .SendAsync("notificationReceived", message, cancellationToken);
  }
}
