using System.Diagnostics;
using System.Reflection;

namespace ChangeMe.Backend.Web.Common;

/// <summary>
/// Adds logging for all messages in the Mediator pipeline.
/// </summary>
public class LoggingBehavior<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
  where TMessage : IMessage
{
  private readonly ILogger<LoggingBehavior<TMessage, TResponse>> _logger;

  public LoggingBehavior(ILogger<LoggingBehavior<TMessage, TResponse>> logger)
  {
    _logger = logger;
  }

  public async ValueTask<TResponse> Handle(
    TMessage message,
    MessageHandlerDelegate<TMessage, TResponse> next,
    CancellationToken cancellationToken)
  {
    if (message is null)
      throw new ArgumentNullException(nameof(message));

    if (_logger.IsEnabled(LogLevel.Information))
    {
      _logger.LogInformation("Handling {MessageName}", typeof(TMessage).Name);

      Type messageType = message.GetType();
      var props = messageType.GetProperties().ToList();
      foreach (PropertyInfo prop in props)
      {
        object? propValue = prop.GetValue(message, null);
        _logger.LogInformation("Property {Property} : {@Value}", prop.Name, propValue);
      }
    }

    var sw = Stopwatch.StartNew();

    var response = await next(message, cancellationToken);

    _logger.LogInformation(
      "Handled {MessageName} with {Response} in {ElapsedMs} ms",
      typeof(TMessage).Name,
      response,
      sw.ElapsedMilliseconds);
    sw.Stop();
    return response;
  }
}
