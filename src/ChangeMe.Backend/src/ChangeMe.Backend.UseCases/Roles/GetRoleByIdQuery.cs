using ChangeMe.Backend.UseCases.Roles.Dtos;

using ChangeMe.Backend.UseCases.Roles.Utils;

namespace ChangeMe.Backend.UseCases.Roles;

public sealed class GetRoleByIdQuery : IQuery<RoleDetailsDto>
{
  public Guid Id { get; set; }
}

public class GetRoleByIdHandler(
  ApplicationDbContext context) : IQueryHandler<GetRoleByIdQuery, RoleDetailsDto>
{
  public async ValueTask<Result<RoleDetailsDto>> Handle(GetRoleByIdQuery query, CancellationToken cancellationToken)
  {
    var role = await context.Roles
      .AsNoTracking()
      .Include(x => x.Permissions)
      .FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken);
    if (role is null)
      return Result<RoleDetailsDto>.NotFound();

    var permissionCodes = role.Permissions.Select(x => x.PermissionCode).ToList();

    var userCount = await context.Users.CountAsync(
      u => u.Roles.Any(ur => ur.RoleId == role.Id),
      cancellationToken);

    return Result.Success(new RoleDetailsDto
    {
      Id = role.Id,
      Name = role.Name,
      Description = role.Description,
      IsSystem = role.IsSystem,
      PermissionCount = permissionCodes.Count,
      UserCount = userCount,
      Version = role.Version,
      Permissions = RolesUtils.MapRolePermissions(permissionCodes)
    });
  }
}
