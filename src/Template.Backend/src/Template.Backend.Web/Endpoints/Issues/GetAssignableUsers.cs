using Template.Backend.UseCases.Issues;
using Template.Backend.UseCases.Issues.Dtos;

namespace Template.Backend.Web.Endpoints.Issues;

public class GetAssignableUsers(IMediator mediator)
  : BaseEndpointWithoutRequest<GetAssignableUsersQuery, List<IssueAssignableUserDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/issues/assignable-users");
    Summary(s =>
    {
      s.Summary = "Get assignable users";
      s.Description = "Gets users that can be assigned to an issue";
    });
  }
}
