using Mediator;

namespace ChangeMe.Backend.UseCases.Common;

public interface IBaseRequestHandler<in TRequest, TResponse> : IRequestHandler<TRequest, Result<TResponse>>
        where TRequest : IBaseRequest<TResponse>;
