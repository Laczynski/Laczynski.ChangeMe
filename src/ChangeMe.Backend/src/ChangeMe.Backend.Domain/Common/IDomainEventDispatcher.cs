namespace ChangeMe.Backend.Domain.Common;

/// <summary>
/// A simple interface for sending domain events. Can use Mediator or any other implementation.
/// </summary>
public interface IDomainEventDispatcher
{
  /// <summary>
  /// Dispatches and clears domain events.
  /// </summary>
  Task DispatchAndClearEvents(IEnumerable<IHasDomainEvents> entitiesWithEvents);
}
