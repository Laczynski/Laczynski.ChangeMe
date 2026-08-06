using ChangeMe.Backend.UseCases.Users;
using ChangeMe.Backend.UseCases.Users.Dtos;
using QueryGrid.Abstractions;

namespace ChangeMe.Backend.Web.Endpoints.Users;

public class GetUsers(IMediator mediator) : BaseEndpoint<GetUsersQuery, GridResult<UserListItemDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.UsersView);
    Get("/users");
    Summary(s => s.Summary = "Get users");
  }
}
