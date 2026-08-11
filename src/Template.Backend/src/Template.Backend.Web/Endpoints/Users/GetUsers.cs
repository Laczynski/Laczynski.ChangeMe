using DataGrid.Abstractions;
using Template.Backend.UseCases.Users;
using Template.Backend.UseCases.Users.Dtos;

namespace Template.Backend.Web.Endpoints.Users;

public class GetUsers(IMediator mediator) : BaseEndpoint<GetUsersQuery, GridResult<UserListItemDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.UsersView);
    Get("/users");
    Summary(s => s.Summary = "Get users");
  }
}
