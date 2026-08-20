namespace ChangeMe.Backend.UseCases.Common;

public interface ICommandHandler<in TCommand, TResponse> : IBaseRequestHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>;
