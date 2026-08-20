using ChangeMe.Backend.UseCases.Roles.Dtos;

using ChangeMe.Backend.UseCases.Roles.Utils;

namespace ChangeMe.Backend.UseCases.Roles;

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
