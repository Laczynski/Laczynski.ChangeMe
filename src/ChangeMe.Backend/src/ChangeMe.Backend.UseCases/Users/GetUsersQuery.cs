using ChangeMe.Backend.UseCases.Users.Dtos;
using DataGrid.Abstractions;
using DataGrid.EntityFrameworkCore;

namespace ChangeMe.Backend.UseCases.Users;

public class GetUsersQuery : IQuery<GridResult<UserListItemDto>>
{
  public GridQuery Grid { get; set; } = new();
}

public class GetUsersHandler(ApplicationDbContext context)
  : IQueryHandler<GetUsersQuery, GridResult<UserListItemDto>>
{
  public async ValueTask<Result<GridResult<UserListItemDto>>> Handle(
    GetUsersQuery query,
    CancellationToken cancellationToken)
  {
    var projectedUsers = context.Users
      .AsNoTracking()
      .Select(u => new UserListItemDto
      {
        Id = u.Id,
        FirstName = u.FirstName,
        LastName = u.LastName,
        Email = u.Email,
        Deactivated = u.Deactivated,
        Status = u.Deactivated ? UserMembershipStatus.Deactivated : UserMembershipStatus.Active,
        RoleNames = u.Roles
          .Select(ur => ur.Role.Name)
          .OrderBy(name => name)
          .ToList(),
        LastSignInAt = context.UserSessions
          .Where(s => s.UserId == u.Id)
          .OrderByDescending(s => s.SignedInAt)
          .Select(s => (DateTime?)s.SignedInAt)
          .FirstOrDefault(),
        CreatedAt = u.CreatedAt
      });

    var grid = await projectedUsers.ToGridResultAsync(query.Grid, cancellationToken: cancellationToken);
    return Result.Success(grid);
  }
}
