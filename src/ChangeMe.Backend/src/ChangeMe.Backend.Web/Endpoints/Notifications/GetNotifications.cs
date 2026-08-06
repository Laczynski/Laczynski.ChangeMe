using ChangeMe.Backend.UseCases.Notifications;
using ChangeMe.Backend.UseCases.Notifications.Dtos;

namespace ChangeMe.Backend.Web.Endpoints.Notifications;

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
