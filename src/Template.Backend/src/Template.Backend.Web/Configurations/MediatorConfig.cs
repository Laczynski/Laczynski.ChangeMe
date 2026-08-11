using Template.Backend.UseCases.Issues;

namespace Template.Backend.Web.Configurations;

public static class MediatorConfig
{
  public static IServiceCollection AddApplicationMediator(this IServiceCollection services)
  {
    services.AddMediator(static options =>
    {
      options.Assemblies = [typeof(GetIssueByIdQuery).Assembly];
      options.ServiceLifetime = ServiceLifetime.Scoped;
      options.PipelineBehaviors = [typeof(LoggingBehavior<,>)];
    });

    services.AddScoped<IDomainEventDispatcher, MediatorDomainEventDispatcher>();

    return services;
  }
}
