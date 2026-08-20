using ChangeMe.Backend.Infrastructure.Persistence;
using ChangeMe.Backend.UseCases.Roles;
using ChangeMe.Backend.UseCases.Users;
using Mediator;

namespace ChangeMe.Backend.UnitTests.Support;

internal abstract class TestMediatorBase : IMediator
{
  public abstract ValueTask<TResponse> Send<TResponse>(
    IRequest<TResponse> request,
    CancellationToken cancellationToken = default);

  public ValueTask<TResponse> Send<TResponse>(
    ICommand<TResponse> command,
    CancellationToken cancellationToken = default) =>
    Send((IRequest<TResponse>)command, cancellationToken);

  public ValueTask<TResponse> Send<TResponse>(
    IQuery<TResponse> query,
    CancellationToken cancellationToken = default) =>
    Send((IRequest<TResponse>)query, cancellationToken);

  public virtual ValueTask<object?> Send(object message, CancellationToken cancellationToken = default) =>
    throw new NotSupportedException();

  public virtual IAsyncEnumerable<TResponse> CreateStream<TResponse>(
    IStreamQuery<TResponse> query,
    CancellationToken cancellationToken = default) =>
    throw new NotSupportedException();

  public virtual IAsyncEnumerable<TResponse> CreateStream<TResponse>(
    IStreamRequest<TResponse> request,
    CancellationToken cancellationToken = default) =>
    throw new NotSupportedException();

  public virtual IAsyncEnumerable<TResponse> CreateStream<TResponse>(
    IStreamCommand<TResponse> command,
    CancellationToken cancellationToken = default) =>
    throw new NotSupportedException();

  public virtual IAsyncEnumerable<object?> CreateStream(
    object message,
    CancellationToken cancellationToken = default) =>
    throw new NotSupportedException();

  public ValueTask Publish<TNotification>(
    TNotification notification,
    CancellationToken cancellationToken = default)
    where TNotification : INotification => default;

  public ValueTask Publish(object notification, CancellationToken cancellationToken = default) => default;
}

internal sealed class UnusedTestMediator : TestMediatorBase
{
  public override ValueTask<TResponse> Send<TResponse>(
    IRequest<TResponse> request,
    CancellationToken cancellationToken = default) =>
    throw new InvalidOperationException();
}

internal sealed class GetUserByIdDispatchingTestMediator(ApplicationDbContext context) : TestMediatorBase
{
  public override async ValueTask<TResponse> Send<TResponse>(
    IRequest<TResponse> request,
    CancellationToken cancellationToken = default)
  {
    if (request is GetUserByIdQuery getUserQuery)
    {
      var handler = new GetUserByIdHandler(context);
      var result = await handler.Handle(getUserQuery, cancellationToken);
      return (TResponse)(object)result!;
    }

    throw new NotSupportedException($"Unsupported request: {request.GetType().Name}");
  }
}

internal sealed class GetRoleByIdDispatchingTestMediator(ApplicationDbContext context) : TestMediatorBase
{
  public override async ValueTask<TResponse> Send<TResponse>(
    IRequest<TResponse> request,
    CancellationToken cancellationToken = default)
  {
    if (request is GetRoleByIdQuery getRoleQuery)
    {
      var handler = new GetRoleByIdHandler(context);
      var result = await handler.Handle(getRoleQuery, cancellationToken);
      return (TResponse)(object)result!;
    }

    throw new NotSupportedException($"Unsupported request: {request.GetType().Name}");
  }
}
