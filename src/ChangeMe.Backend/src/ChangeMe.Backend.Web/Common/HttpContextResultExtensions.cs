namespace ChangeMe.Backend.Web.Common;

public static class HttpContextResultExtensions
{
  public static async Task SendResultAsync<T>(
    this HttpContext httpContext,
    Result<T> result,
    CancellationToken cancellationToken)
  {
    try
    {
      var statusCode = ResultHttpMapper.MapStatusCode(result.Status);
      if (statusCode == StatusCodes.Status204NoContent)
      {
        await httpContext.Response.SendNoContentAsync(cancellationToken);
        return;
      }

      await httpContext.Response.SendAsync(result, statusCode, cancellation: cancellationToken);
    }
    catch (OperationCanceledException)
    {
      // Client disconnected or request was cancelled - this is expected behavior.
    }
  }
}
