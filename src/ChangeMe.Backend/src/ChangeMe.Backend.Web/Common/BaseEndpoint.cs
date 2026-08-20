using ChangeMe.Backend.Web.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace ChangeMe.Backend.Web.Common;

public abstract class BaseEndpoint<TRequest, TResponse>(IMediator mediator) : Endpoint<TRequest, Result<TResponse>>
    where TRequest : notnull
{
  public override void Configure()
  {
    DontThrowIfValidationFails();
    AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    Version(ApiVersionConfig.CurrentVersion);
    ConfigureEndpoint();
  }

  protected virtual void ConfigureEndpoint()
  {
  }

  protected void RequirePermission(string permissionCode) => Policies(permissionCode);

  protected void RequirePermissions(params string[] permissionCodes) => Policies(permissionCodes);

  public override async Task HandleAsync(TRequest req, CancellationToken ct)
  {
    Result<TResponse> response = ValidationFailed switch
    {
      true => Result<TResponse>.Invalid(ValidationFailures.Select(f => new ValidationError
      {
        Identifier = f.PropertyName,
        ErrorMessage = f.ErrorMessage,
        Severity = ValidationSeverity.Error
      }).ToArray()),
      false => await mediator.Send(req, ct) as Result<TResponse> ?? Result<TResponse>.Error("Unknown error")
    };

    await HttpContext.SendResultAsync(response, ct);
  }
}
