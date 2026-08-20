using ChangeMe.Backend.UseCases.Auth.Dtos;
using ChangeMe.Backend.UseCases.Users.Dtos;
using ChangeMe.Backend.UseCases.Users.Utils;

namespace ChangeMe.Backend.UseCases.Auth;

public sealed record GetMyAccountQuery() : IQuery<MyAccountDto>;

public class GetMyAccountHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetMyAccountQuery, MyAccountDto>
{
  public async ValueTask<Result<MyAccountDto>> Handle(GetMyAccountQuery query, CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid userId)
      return Result<MyAccountDto>.Unauthorized();

    var user = await context.Users
      .AsNoTracking()
      .Include(x => x.Roles)
      .ThenInclude(x => x.Role)
      .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
    if (user is null || !user.IsActive)
      return Result<MyAccountDto>.Unauthorized();

    var roles = user.Roles
      .Select(x => x.Role)
      .OrderBy(role => role.Name)
      .Select(role => new UserRoleSummaryDto(role.Id, role.Name, role.IsSystem))
      .ToList();

    var effectivePermissions = await UsersUtils.GetEffectivePermissionsForUserAsync(
      context,
      userId,
      cancellationToken);

    return Result.Success(new MyAccountDto(
      user.Id,
      user.FirstName,
      user.LastName,
      user.Email,
      user.CreatedAt,
      user.Version,
      roles,
      effectivePermissions));
  }
}
