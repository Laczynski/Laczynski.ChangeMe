using ChangeMe.Backend.UseCases.Users.Dtos;

namespace ChangeMe.Backend.UseCases.Users;

public sealed record GetRolesForAssignmentQuery() : IQuery<IReadOnlyList<RoleAssignmentOptionDto>>;

public class GetRolesForAssignmentHandler(ApplicationDbContext context)
  : IQueryHandler<GetRolesForAssignmentQuery, IReadOnlyList<RoleAssignmentOptionDto>>
{
  public async ValueTask<Result<IReadOnlyList<RoleAssignmentOptionDto>>> Handle(
    GetRolesForAssignmentQuery query,
    CancellationToken cancellationToken)
  {
    var roles = await context.Roles
      .AsNoTracking()
      .OrderBy(x => x.Name)
      .Select(x => new RoleAssignmentOptionDto(x.Id, x.Name, x.IsSystem))
      .ToListAsync(cancellationToken);

    return Result.Success<IReadOnlyList<RoleAssignmentOptionDto>>(roles);
  }
}
