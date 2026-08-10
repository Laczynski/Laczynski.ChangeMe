using Template.Backend.UseCases.Issues;

namespace Template.Backend.Web.Endpoints.Issues;

public class WatchIssue(IMediator mediator) : BaseEndpoint<WatchIssueCommand, IssueWatchStateDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/issues/{issueId}/watch");
    Summary(s =>
    {
      s.Summary = "Watch issue";
      s.Description = "Starts watching an issue";
    });
  }
}

public class UnwatchIssue(IMediator mediator) : BaseEndpoint<UnwatchIssueCommand, IssueWatchStateDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Delete("/issues/{issueId}/watch");
    Summary(s =>
    {
      s.Summary = "Unwatch issue";
      s.Description = "Stops watching an issue";
    });
  }
}

public sealed class WatchIssueCommandValidator : Validator<WatchIssueCommand>
{
  public WatchIssueCommandValidator()
  {
    RuleFor(x => x.IssueId)
      .NotEmpty();
  }
}

public sealed class UnwatchIssueCommandValidator : Validator<UnwatchIssueCommand>
{
  public UnwatchIssueCommandValidator()
  {
    RuleFor(x => x.IssueId)
      .NotEmpty();
  }
}
