using Mediator;

namespace ChangeMe.Backend.UseCases.Common;

public interface IBaseRequest<TResponse> : IRequest<Result<TResponse>>;
