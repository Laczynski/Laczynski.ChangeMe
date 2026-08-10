using Mediator;

namespace Template.Backend.UseCases.Common;

public interface IBaseRequestHandler<in TRequest, TResponse> : IRequestHandler<TRequest, Result<TResponse>>
        where TRequest : IBaseRequest<TResponse>;
