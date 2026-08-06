using ChangeMe.Backend.UseCases.Users;
using ChangeMe.Backend.UseCases.Users.Dtos;

namespace ChangeMe.Backend.Web.Endpoints.Users;

public class DeactivateUser(IMediator mediator) : BaseEndpoint<DeactivateUserCommand, UserDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.UsersDeactivate);
    Post("/users/{Id}/deactivate");
    Summary(s => s.Summary = "Deactivate user");
  }
}
