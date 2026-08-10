using Template.Backend.UseCases.Users;
using Template.Backend.UseCases.Users.Dtos;

namespace Template.Backend.Web.Endpoints.Users;

public class GetUserById(IMediator mediator) : BaseEndpoint<GetUserByIdQuery, UserDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.UsersView);
    Get("/users/{Id}");
    Summary(s => s.Summary = "Get user details");
  }
}
