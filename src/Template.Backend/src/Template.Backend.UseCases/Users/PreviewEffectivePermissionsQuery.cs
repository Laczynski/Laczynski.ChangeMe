using Template.Backend.UseCases.Users.Dtos;
using Template.Backend.UseCases.Users.Utils;

namespace Template.Backend.UseCases.Users;

public sealed class PreviewEffectivePermissionsQuery : IQuery<IReadOnlyList<EffectivePermissionDto>>
{
  public List<Guid> RoleIds { get; set; } = [];
}

public class PreviewEffectivePermissionsHandler(ApplicationDbContext context)
  : IQueryHandler<PreviewEffectivePermissionsQuery, IReadOnlyList<EffectivePermissionDto>>
{
  public async ValueTask<Result<IReadOnlyList<EffectivePermissionDto>>> Handle(
    PreviewEffectivePermissionsQuery query,
    CancellationToken cancellationToken)
  {
    var permissions = await UsersUtils.GetEffectivePermissionsForRolesAsync(
      context,
      query.RoleIds,
      cancellationToken);

    return Result.Success(permissions);
  }
}
