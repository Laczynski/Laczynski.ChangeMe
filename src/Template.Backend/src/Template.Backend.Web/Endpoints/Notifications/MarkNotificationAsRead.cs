using Template.Backend.UseCases.Notifications;
using Template.Backend.UseCases.Notifications.Dtos;

namespace Template.Backend.Web.Endpoints.Notifications;

public class MarkNotificationAsRead(IMediator mediator) : BaseEndpoint<MarkNotificationAsReadCommand, NotificationDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Put("/notifications/{notificationId}/read");
    Summary(s =>
    {
      s.Summary = "Mark notification as read";
      s.Description = "Marks a single notification as read";
    });
  }
}

public sealed class MarkNotificationAsReadCommandValidator : Validator<MarkNotificationAsReadCommand>
{
  public MarkNotificationAsReadCommandValidator()
  {
    RuleFor(x => x.NotificationId)
      .NotEmpty();
  }
}
