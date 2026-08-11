using Template.Backend.UseCases.Auth;
using Template.Backend.UseCases.Auth.Dtos;

namespace Template.Backend.Web.Endpoints.Auth;

public class GetMyAccount(IMediator mediator) : BaseEndpointWithoutRequest<GetMyAccountQuery, MyAccountDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/auth/account");
    Summary(s =>
    {
      s.Summary = "Get my account";
      s.Description = "Returns the signed-in user's profile and effective permissions.";
    });
  }
}
