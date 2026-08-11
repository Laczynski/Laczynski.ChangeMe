using DataGrid.Abstractions;
using DataGrid.EntityFrameworkCore;
using Template.Backend.UseCases.Roles.Dtos;

namespace Template.Backend.UseCases.Roles;

public sealed class GetRolesQuery : IQuery<GridResult<RoleListItemDto>>
{
  public GridQuery Grid { get; set; } = new();
}

public class GetRolesHandler(ApplicationDbContext context)
  : IQueryHandler<GetRolesQuery, GridResult<RoleListItemDto>>
{
  public async ValueTask<Result<GridResult<RoleListItemDto>>> Handle(
    GetRolesQuery query,
    CancellationToken cancellationToken)
  {
    var projected = context.Roles
      .AsNoTracking()
      .Select(r => new RoleListItemDto
      {
        Id = r.Id,
        Name = r.Name,
        Description = r.Description,
        PermissionCount = r.Permissions.Count,
        UserCount = context.Users.Count(u => u.Roles.Any(ur => ur.RoleId == r.Id)),
        IsSystem = r.IsSystem
      });

    var grid = await projected.ToGridResultAsync(query.Grid, cancellationToken: cancellationToken);
    return Result.Success(grid);
  }
}
