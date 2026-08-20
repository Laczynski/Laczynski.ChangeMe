using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace ChangeMe.Backend.Web.Configurations;

public static class ExceptionHandlerConfig
{
  public static WebApplication UseExceptionHandler(this WebApplication app)
  {
    app.UseExceptionHandler(appError =>
    {
      appError.Run(async context =>
          {
            context.Response.ContentType = "application/json";

            var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
            if (contextFeature != null)
            {
              var exception = contextFeature.Error;

              var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
              logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

              var (statusCode, message) = exception switch
              {
                DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, ConcurrencyMessages.StaleVersion),
                ArgumentException => (StatusCodes.Status400BadRequest, "Bad Request"),
                KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
              };

              context.Response.StatusCode = statusCode;

              var response = statusCode == StatusCodes.Status409Conflict
                ? Result<object>.Conflict(message)
                : Result<object>.Error(message);

              await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
          });
    });
    return app;
  }
}
