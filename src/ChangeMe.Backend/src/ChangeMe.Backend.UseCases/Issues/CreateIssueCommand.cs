using ChangeMe.Backend.Domain.Aggregates.Issue;
using ChangeMe.Backend.Domain.Aggregates.Issue.Enums;
using ChangeMe.Backend.UseCases.Issues.Dtos;
using ChangeMe.Backend.UseCases.Issues.Utils;

namespace ChangeMe.Backend.UseCases.Issues;

public record CreateIssueCommand(
  string Title,
  string Description,
  IssueStatus Status,
  IssuePriority Priority,
  Guid? AssignedToUserId,
  bool WatchAfterCreate,
  List<CreateIssueAcceptanceCriterionPayload>? AcceptanceCriteria = null) : ICommand<IssueDetailsDto>;

public record CreateIssueAcceptanceCriterionPayload(string Content);

public class CreateIssueHandler(
  IMediator mediator,
  ApplicationDbContext context,
  IUserAccessor userAccessor) : ICommandHandler<CreateIssueCommand, IssueDetailsDto>
{
  public async ValueTask<Result<IssueDetailsDto>> Handle(CreateIssueCommand command, CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid actorUserId)
      return Result.Unauthorized();

    var assigneeValidation = await IssuesUtils.ValidateAssigneeExistsAsync(
      context,
      command.AssignedToUserId,
      nameof(command.AssignedToUserId),
      cancellationToken);
    if (!assigneeValidation.IsSuccess)
      return assigneeValidation.Map();

    var issueResult = Issue.Create(
      command.Title,
      command.Description,
      command.Priority,
      command.Status,
      command.AssignedToUserId);

    if (!issueResult.IsSuccess)
      return issueResult.Map();

    var creationResult = issueResult.Value.RecordCreation(actorUserId);
    if (!creationResult.IsSuccess)
      return creationResult.Map();

    foreach (var acceptanceCriterion in command.AcceptanceCriteria ?? [])
    {
      var acceptanceCriterionResult = issueResult.Value.AddAcceptanceCriterion(acceptanceCriterion.Content);
      if (!acceptanceCriterionResult.IsSuccess)
        return acceptanceCriterionResult.Map();

      await context.IssueAcceptanceCriteria.AddAsync(acceptanceCriterionResult.Value, cancellationToken);
    }

    if (command.WatchAfterCreate)
    {
      var watchResult = issueResult.Value.StartWatching(actorUserId);
      if (!watchResult.IsSuccess)
        return watchResult.Map();

      await context.IssueWatchers.AddAsync(watchResult.Value, cancellationToken);
    }

    await context.Issues.AddAsync(issueResult.Value, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    var createdIssueResult = await mediator.Send(new GetIssueByIdQuery(issueResult.Value.Id), cancellationToken);
    if (!createdIssueResult.IsSuccess)
      return createdIssueResult.Map();

    return Result.Created(createdIssueResult.Value, $"/issues/{createdIssueResult.Value.Id}");
  }
}
