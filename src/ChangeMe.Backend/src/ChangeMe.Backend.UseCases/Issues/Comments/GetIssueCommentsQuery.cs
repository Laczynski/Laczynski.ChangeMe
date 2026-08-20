using ChangeMe.Backend.UseCases.Issues.Dtos;
using ChangeMe.Backend.UseCases.Issues.Utils;
using DataGrid.Abstractions;
using DataGrid.EntityFrameworkCore;

namespace ChangeMe.Backend.UseCases.Issues.Comments;

public sealed class GetIssueCommentsQuery : IQuery<GridResult<IssueCommentDto>>
{
  public Guid IssueId { get; set; }
  public GridQuery Grid { get; set; } = new();
}

public class GetIssueCommentsHandler(ApplicationDbContext context)
  : IQueryHandler<GetIssueCommentsQuery, GridResult<IssueCommentDto>>
{
  public async ValueTask<Result<GridResult<IssueCommentDto>>> Handle(
    GetIssueCommentsQuery query,
    CancellationToken cancellationToken)
  {
    var issueExists = await context.Issues.AsNoTracking().AnyAsync(i => i.Id == query.IssueId, cancellationToken);
    if (!issueExists)
      return Result<GridResult<IssueCommentDto>>.NotFound();

    var projected = context.IssueComments
      .AsNoTracking()
      .Where(c => c.IssueId == query.IssueId)
      .Select(c => new IssueCommentDto
      {
        Id = c.Id,
        Content = c.Content,
        AuthorUserId = c.CreatedBy,
        CreatedAt = c.CreatedAt
      });

    var grid = await projected.ToGridResultAsync(query.Grid, cancellationToken: cancellationToken);

    var userLookup = await IssuesUtils.GetUserDisplayNameLookupAsync(
      context,
      grid.Items.Select(c => c.AuthorUserId),
      cancellationToken);

    foreach (var item in grid.Items)
      item.AuthorName = userLookup.GetValueOrDefault(item.AuthorUserId);

    return Result.Success(grid);
  }
}
