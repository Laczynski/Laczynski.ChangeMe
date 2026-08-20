namespace ChangeMe.Backend.UseCases.Common;

public interface IQueryHandler<in TQuery, TResponse> : IBaseRequestHandler<TQuery, TResponse>
       where TQuery : IQuery<TResponse>;
