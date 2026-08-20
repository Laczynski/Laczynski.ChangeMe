using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.UseCases.Issues.Dtos;
using DataGrid.Abstractions;
using DataGrid.EntityFrameworkCore;

namespace ChangeMe.Backend.UseCases.Issues;

public class GetAllIssuesQuery : IQuery<GridResult<IssueDto>>
{
  public GridQuery Grid { get; set; } = new();
}

public class GetAllIssuesHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetAllIssuesQuery, GridResult<IssueDto>>
{
  public async ValueTask<Result<GridResult<IssueDto>>> Handle(GetAllIssuesQuery query, CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid currentUserId)
      return Result.Unauthorized();

    var projectedIssues = context.Issues
      .AsNoTracking()
      .Select(i => new IssueDto
      {
        Id = i.Id,
        Title = i.Title,
        Description = i.Description,
        Status = i.Status,
        Priority = i.Priority,
        CreatedBy = i.CreatedBy,
        CreatedByName = context.Users
          .Where(u => u.Id == i.CreatedBy)
          .Select(UserDisplayFormat.DisplayLabelExpression)
          .FirstOrDefault(),
        AssignedToUserId = i.AssignedToUserId,
        AssignedToUserName = i.AssignedToUserId.HasValue
          ? context.Users
            .Where(u => u.Id == i.AssignedToUserId.Value)
            .Select(UserDisplayFormat.DisplayLabelExpression)
            .FirstOrDefault()
          : null,
        CreatedAt = i.CreatedAt,
        UpdatedAt = i.UpdatedAt,
        LastActivityAt = i.LastActivityAt,
        IsWatchedByCurrentUser = i.Watchers.Any(w => w.UserId == currentUserId),
        WatchersCount = i.Watchers.Count,
      });

    var grid = await projectedIssues.ToGridResultAsync(query.Grid, cancellationToken: cancellationToken);
    return Result.Success(grid);
  }
}
