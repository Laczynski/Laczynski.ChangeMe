using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using ChangeMe.Backend.UseCases.Users.Utils;
using QueryGrid.Abstractions;
using QueryGrid.EntityFrameworkCore;

namespace ChangeMe.Backend.UseCases.Auth;

public sealed class GetMySessionsQuery : IQuery<GridResult<UserSessionDto>>
{
  public GridQuery Grid { get; set; } = new();
}

public class GetMySessionsHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  ISessionLifetimeService sessionLifetime) : IQueryHandler<GetMySessionsQuery, GridResult<UserSessionDto>>
{
  public async ValueTask<Result<GridResult<UserSessionDto>>> Handle(
    GetMySessionsQuery query,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid userId)
      return Result<GridResult<UserSessionDto>>.Unauthorized();

    var utcNow = DateTime.UtcNow;
    var currentSessionId = userAccessor.SessionId;
    var sessionLifetimeDays = sessionLifetime.SessionLifetimeDays;

    var sessions = context.UserSessions
      .AsNoTracking()
      .WhereActiveSessions(userId, utcNow, sessionLifetimeDays);

    var grid = await sessions.ToGridResultAsync(query.Grid, cancellationToken: cancellationToken);

    var items = grid.Items
      .Select(x => new UserSessionDto(
        x.Id,
        x.DeviceBrowserLabel,
        x.SignInMethod,
        x.IpAddress,
        x.SignedInAt,
        x.LastActivityAt,
        currentSessionId.HasValue && x.Id == currentSessionId.Value))
      .ToList();

    return Result.Success(new GridResult<UserSessionDto>
    {
      Items = items,
      TotalCount = grid.TotalCount,
      Skip = grid.Skip,
      Take = grid.Take,
      Sort = grid.Sort
    });
  }
}
