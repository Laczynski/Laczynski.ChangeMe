using Template.Backend.UseCases.Users;
using Template.Backend.UseCases.Users.Dtos;

namespace Template.Backend.Web.Endpoints.Users;

public class DeactivateUser(IMediator mediator) : BaseEndpoint<DeactivateUserCommand, UserDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.UsersDeactivate);
    Post("/users/{Id}/deactivate");
    Summary(s => s.Summary = "Deactivate user");
  }
}
