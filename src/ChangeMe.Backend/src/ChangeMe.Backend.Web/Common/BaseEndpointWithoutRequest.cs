using ChangeMe.Backend.Web.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace ChangeMe.Backend.Web.Common;

public abstract class BaseEndpointWithoutRequest<TRequest, TResponse>(IMediator mediator)
  : EndpointWithoutRequest<Result<TResponse>>
  where TRequest : notnull, IRequest<Result<TResponse>>, new()
{
  public override void Configure()
  {
    AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    Version(ApiVersionConfig.CurrentVersion);
    ConfigureEndpoint();
  }

  protected virtual void ConfigureEndpoint()
  {
  }

  protected void RequirePermission(string permissionCode) => Policies(permissionCode);

  protected void RequirePermissions(params string[] permissionCodes) => Policies(permissionCodes);

  protected virtual TRequest CreateRequest() => new();

  public override async Task HandleAsync(CancellationToken ct)
  {
    var response = await mediator.Send(CreateRequest(), ct)
      ?? Result<TResponse>.Error("Unknown error");

    await HttpContext.SendResultAsync(response, ct);
  }
}
