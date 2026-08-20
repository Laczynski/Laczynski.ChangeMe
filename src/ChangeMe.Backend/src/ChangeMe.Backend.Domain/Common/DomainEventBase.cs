using Mediator;

namespace ChangeMe.Backend.Domain.Common;

/// <summary>
/// A base type for domain events. Depends on Mediator INotification.
/// Includes DateOccurred which is set on creation.
/// </summary>
public abstract class DomainEventBase : Entity, INotification
{
  /// <summary>
  /// The date and time the event occurred.
  /// </summary>
  public DateTime DateOccurred { get; protected set; } = DateTime.UtcNow;
}

