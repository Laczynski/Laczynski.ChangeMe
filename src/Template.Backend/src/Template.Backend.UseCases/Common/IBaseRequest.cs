using Mediator;

namespace Template.Backend.UseCases.Common;

public interface IBaseRequest<TResponse> : IRequest<Result<TResponse>>;
