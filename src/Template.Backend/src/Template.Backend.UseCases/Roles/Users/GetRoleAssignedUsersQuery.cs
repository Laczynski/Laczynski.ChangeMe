using DataGrid.Abstractions;
using DataGrid.EntityFrameworkCore;
using Template.Backend.UseCases.Roles.Dtos;

namespace Template.Backend.UseCases.Roles.Users;

public sealed class GetRoleAssignedUsersQuery : IQuery<GridResult<RoleAssignedUserDto>>
{
  public Guid RoleId { get; set; }
  public GridQuery Grid { get; set; } = new();
}

public class GetRoleAssignedUsersHandler(ApplicationDbContext context)
  : IQueryHandler<GetRoleAssignedUsersQuery, GridResult<RoleAssignedUserDto>>
{
  public async ValueTask<Result<GridResult<RoleAssignedUserDto>>> Handle(
    GetRoleAssignedUsersQuery query,
    CancellationToken cancellationToken)
  {
    var roleExists = await context.Roles.AsNoTracking().AnyAsync(x => x.Id == query.RoleId, cancellationToken);
    if (!roleExists)
      return Result<GridResult<RoleAssignedUserDto>>.NotFound();

    var projected = context.Users
      .AsNoTracking()
      .Where(u => u.Roles.Any(ur => ur.RoleId == query.RoleId))
      .Select(u => new RoleAssignedUserDto
      {
        Id = u.Id,
        FirstName = u.FirstName,
        LastName = u.LastName,
        Email = u.Email,
        Deactivated = u.Deactivated
      });

    var grid = await projected.ToGridResultAsync(query.Grid, cancellationToken: cancellationToken);
    return Result.Success(grid);
  }
}
