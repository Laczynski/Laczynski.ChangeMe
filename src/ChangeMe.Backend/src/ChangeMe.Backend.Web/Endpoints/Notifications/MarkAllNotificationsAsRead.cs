using ChangeMe.Backend.UseCases.Notifications;

namespace ChangeMe.Backend.Web.Endpoints.Notifications;

public class MarkAllNotificationsAsRead(IMediator mediator)
  : BaseEndpointWithoutRequest<MarkAllNotificationsAsReadCommand, bool>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Put("/notifications/read-all");
    Summary(s =>
    {
      s.Summary = "Mark all notifications as read";
      s.Description = "Marks all notifications for current user as read";
    });
  }
}
