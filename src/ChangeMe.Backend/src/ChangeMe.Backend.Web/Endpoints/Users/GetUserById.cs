using ChangeMe.Backend.UseCases.Users;
using ChangeMe.Backend.UseCases.Users.Dtos;

namespace ChangeMe.Backend.Web.Endpoints.Users;

public class GetUserById(IMediator mediator) : BaseEndpoint<GetUserByIdQuery, UserDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.UsersView);
    Get("/users/{Id}");
    Summary(s => s.Summary = "Get user details");
  }
}
