using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace ChangeMe.Backend.Web.Authorization;

public sealed class PermissionDeniedAuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
{
  private readonly AuthorizationMiddlewareResultHandler defaultHandler = new();

  public async Task HandleAsync(
    RequestDelegate next,
    HttpContext context,
    AuthorizationPolicy policy,
    PolicyAuthorizationResult authorizeResult)
  {
    if (authorizeResult.Forbidden && !authorizeResult.Succeeded)
    {
      context.Response.StatusCode = StatusCodes.Status403Forbidden;
      await context.Response.WriteAsJsonAsync(
        Result.Forbidden(PermissionAuthorizationExtensions.PermissionDeniedMessage),
        cancellationToken: context.RequestAborted);
      return;
    }

    await defaultHandler.HandleAsync(next, context, policy, authorizeResult);
  }
}
