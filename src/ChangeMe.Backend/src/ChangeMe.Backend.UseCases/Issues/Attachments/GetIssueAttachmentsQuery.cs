using ChangeMe.Backend.UseCases.Issues.Dtos;
using ChangeMe.Backend.UseCases.Issues.Utils;
using DataGrid.Abstractions;
using DataGrid.EntityFrameworkCore;

namespace ChangeMe.Backend.UseCases.Issues.Attachments;

public sealed class GetIssueAttachmentsQuery : IQuery<GridResult<IssueAttachmentDto>>
{
  public Guid IssueId { get; set; }
  public GridQuery Grid { get; set; } = new();
}

public class GetIssueAttachmentsHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetIssueAttachmentsQuery, GridResult<IssueAttachmentDto>>
{
  public async ValueTask<Result<GridResult<IssueAttachmentDto>>> Handle(
    GetIssueAttachmentsQuery query,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid currentUserId)
      return Result<GridResult<IssueAttachmentDto>>.Unauthorized();

    var issueExists = await context.Issues.AsNoTracking().AnyAsync(i => i.Id == query.IssueId, cancellationToken);
    if (!issueExists)
      return Result<GridResult<IssueAttachmentDto>>.NotFound();

    var projected = context.IssueAttachments
      .AsNoTracking()
      .Where(a => a.OwnerId == query.IssueId)
      .Select(a => new IssueAttachmentDto
      {
        Id = a.Id,
        OriginalFileName = a.OriginalFileName,
        ContentType = a.ContentType,
        SizeBytes = a.SizeBytes,
        UploadedByUserId = a.CreatedBy,
        CreatedAt = a.CreatedAt,
        CanDelete = a.CreatedBy == currentUserId
      });

    var grid = await projected.ToGridResultAsync(query.Grid, cancellationToken: cancellationToken);

    var userLookup = await IssuesUtils.GetUserDisplayNameLookupAsync(
      context,
      grid.Items.Select(a => a.UploadedByUserId),
      cancellationToken);

    foreach (var item in grid.Items)
      item.UploadedByName = userLookup.GetValueOrDefault(item.UploadedByUserId);

    return Result.Success(grid);
  }
}
