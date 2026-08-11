using Template.Backend.UseCases.Notifications;
using Template.Backend.UseCases.Notifications.Dtos;

namespace Template.Backend.Web.Endpoints.Notifications;

public class GetNotifications(IMediator mediator) : BaseEndpoint<GetNotificationsQuery, NotificationListDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/notifications");
    Summary(s =>
    {
      s.Summary = "Get notifications";
      s.Description = "Gets notifications for current user";
    });
  }
}
