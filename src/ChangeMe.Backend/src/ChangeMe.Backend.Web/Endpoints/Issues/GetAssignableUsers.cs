using ChangeMe.Backend.UseCases.Issues;
using ChangeMe.Backend.UseCases.Issues.Dtos;

namespace ChangeMe.Backend.Web.Endpoints.Issues;

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
