using ChangeMe.Backend.UseCases.Users;
using ChangeMe.Backend.UseCases.Users.Dtos;

namespace ChangeMe.Backend.Web.Endpoints.Users;

public class ActivateUser(IMediator mediator) : BaseEndpoint<ActivateUserCommand, UserDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.UsersDeactivate);
    Post("/users/{Id}/activate");
    Summary(s => s.Summary = "Activate user");
  }
}
