namespace ChangeMe.Backend.UseCases.Issues;

public record WatchIssueCommand(Guid IssueId) : ICommand<IssueWatchStateDto>;
public record UnwatchIssueCommand(Guid IssueId) : ICommand<IssueWatchStateDto>;

public class IssueWatchStateDto
{
  public Guid IssueId { get; set; }
  public bool IsWatchedByCurrentUser { get; set; }
  public int WatchersCount { get; set; }
}

public class WatchIssueHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : ICommandHandler<WatchIssueCommand, IssueWatchStateDto>
{
  public async ValueTask<Result<IssueWatchStateDto>> Handle(WatchIssueCommand command, CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid actorUserId)
      return Result.Unauthorized();

    var issue = await context.Issues
      .Include(i => i.Watchers)
      .FirstOrDefaultAsync(i => i.Id == command.IssueId, cancellationToken);

    if (issue is null)
      return Result.NotFound();

    var watchResult = issue.StartWatching(actorUserId);
    if (!watchResult.IsSuccess)
      return watchResult.Map();

    await context.IssueWatchers.AddAsync(watchResult.Value, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    return Result.Success(new IssueWatchStateDto
    {
      IssueId = issue.Id,
      IsWatchedByCurrentUser = true,
      WatchersCount = issue.Watchers.Count,
    });
  }
}

public class UnwatchIssueHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : ICommandHandler<UnwatchIssueCommand, IssueWatchStateDto>
{
  public async ValueTask<Result<IssueWatchStateDto>> Handle(UnwatchIssueCommand command, CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid actorUserId)
      return Result<IssueWatchStateDto>.Unauthorized();

    var issue = await context.Issues
      .Include(i => i.Watchers)
      .FirstOrDefaultAsync(i => i.Id == command.IssueId, cancellationToken);

    if (issue is null)
      return Result.NotFound();

    var unwatchResult = issue.StopWatching(actorUserId);
    if (!unwatchResult.IsSuccess)
      return unwatchResult.Map();

    context.IssueWatchers.Remove(unwatchResult.Value);
    await context.SaveChangesAsync(cancellationToken);

    return Result.Success(new IssueWatchStateDto
    {
      IssueId = issue.Id,
      IsWatchedByCurrentUser = false,
      WatchersCount = issue.Watchers.Count,
    });
  }
}
