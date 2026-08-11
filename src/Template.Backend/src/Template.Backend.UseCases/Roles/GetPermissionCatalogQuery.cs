using Template.Backend.UseCases.Roles.Dtos;

using Template.Backend.UseCases.Roles.Utils;

namespace Template.Backend.UseCases.Roles;

public sealed record GetPermissionCatalogQuery() : IQuery<IReadOnlyList<PermissionCatalogItemDto>>;

public class GetPermissionCatalogHandler()
  : IQueryHandler<GetPermissionCatalogQuery, IReadOnlyList<PermissionCatalogItemDto>>
{
  public async ValueTask<Result<IReadOnlyList<PermissionCatalogItemDto>>> Handle(
    GetPermissionCatalogQuery query,
    CancellationToken cancellationToken)
  {
    return Result.Success(RolesUtils.GetPermissionCatalog());
  }
}
