using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Users.Dtos;
using ChangeMe.Backend.UseCases.Users.Utils;
using QueryGrid.Abstractions;
using QueryGrid.EntityFrameworkCore;

namespace ChangeMe.Backend.UseCases.Users;

public sealed class GetUserSessionsQuery : IQuery<GridResult<AdminUserSessionDto>>
{
  public Guid Id { get; set; }
  public GridQuery Grid { get; set; } = new();
}

public class GetUserSessionsHandler(
  ApplicationDbContext context,
  ISessionLifetimeService sessionLifetime) : IQueryHandler<GetUserSessionsQuery, GridResult<AdminUserSessionDto>>
{
  public async ValueTask<Result<GridResult<AdminUserSessionDto>>> Handle(
    GetUserSessionsQuery query,
    CancellationToken cancellationToken)
  {
    var userExists = await context.Users.AnyAsync(x => x.Id == query.Id, cancellationToken);
    if (!userExists)
      return Result<GridResult<AdminUserSessionDto>>.NotFound();

    var user = await context.Users.AsNoTracking().FirstAsync(x => x.Id == query.Id, cancellationToken);
    if (!user.IsActive)
      return Result.Success(new GridResult<AdminUserSessionDto>());

    var utcNow = DateTime.UtcNow;
    var sessionLifetimeDays = sessionLifetime.SessionLifetimeDays;

    var sessions = context.UserSessions
      .AsNoTracking()
      .WhereActiveSessions(query.Id, utcNow, sessionLifetimeDays);

    var grid = await sessions.ToGridResultAsync(query.Grid, cancellationToken: cancellationToken);

    var items = grid.Items
      .Select(x => new AdminUserSessionDto(
        x.Id,
        x.DeviceBrowserLabel,
        x.SignInMethod,
        x.IpAddress,
        x.SignedInAt,
        x.LastActivityAt))
      .ToList();

    return Result.Success(new GridResult<AdminUserSessionDto>
    {
      Items = items,
      TotalCount = grid.TotalCount,
      Skip = grid.Skip,
      Take = grid.Take,
      Sort = grid.Sort
    });
  }
}
