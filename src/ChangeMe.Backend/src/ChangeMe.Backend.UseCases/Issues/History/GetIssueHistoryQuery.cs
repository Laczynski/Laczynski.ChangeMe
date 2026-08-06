using ChangeMe.Backend.UseCases.Issues.Dtos;
using ChangeMe.Backend.UseCases.Issues.Utils;
using QueryGrid.Abstractions;
using QueryGrid.EntityFrameworkCore;

namespace ChangeMe.Backend.UseCases.Issues.History;

public sealed class GetIssueHistoryQuery : IQuery<GridResult<IssueHistoryEntryDto>>
{
  public Guid IssueId { get; set; }
  public GridQuery Grid { get; set; } = new();
}

public class GetIssueHistoryHandler(ApplicationDbContext context)
  : IQueryHandler<GetIssueHistoryQuery, GridResult<IssueHistoryEntryDto>>
{
  public async ValueTask<Result<GridResult<IssueHistoryEntryDto>>> Handle(
    GetIssueHistoryQuery query,
    CancellationToken cancellationToken)
  {
    var issueExists = await context.Issues.AsNoTracking().AnyAsync(i => i.Id == query.IssueId, cancellationToken);
    if (!issueExists)
      return Result<GridResult<IssueHistoryEntryDto>>.NotFound();

    var projected = context.IssueHistoryEntries
      .AsNoTracking()
      .Where(h => h.IssueId == query.IssueId)
      .Select(h => new IssueHistoryEntryDto
      {
        Id = h.Id,
        EventType = h.EventType,
        ActorUserId = h.ActorUserId,
        Summary = h.Summary,
        PreviousValue = h.PreviousValue,
        CurrentValue = h.CurrentValue,
        CreatedAt = h.CreatedAt
      });

    var grid = await projected.ToGridResultAsync(query.Grid, cancellationToken: cancellationToken);

    var userLookup = await IssuesUtils.GetUserDisplayNameLookupAsync(
      context,
      IssueMappingExtensions.CollectHistoryRelatedUserIds(grid.Items),
      cancellationToken);

    var items = grid.Items
      .Select(entry => entry.ToHistoryEntryDto(userLookup))
      .ToList();

    return Result.Success(new GridResult<IssueHistoryEntryDto>
    {
      Items = items,
      TotalCount = grid.TotalCount,
      Skip = grid.Skip,
      Take = grid.Take,
      Sort = grid.Sort
    });
  }
}
