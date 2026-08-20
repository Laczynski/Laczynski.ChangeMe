namespace ChangeMe.Backend.Domain.Common;

public abstract class Entity : HasDomainEventsBase
{
  public Guid Id { get; set; } = Guid.CreateVersion7();

  public Guid CreatedBy { get; set; }
  public Guid? UpdatedBy { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime? UpdatedAt { get; set; }
  public long Version { get; set; }

  public bool IsDeleted { get; set; } = false;
}

/// <summary>
/// For use with Vogen or similar tools for generating code for
/// strongly typed Ids.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="TId"></typeparam>
public abstract class EntityBase<T, TId> : HasDomainEventsBase
  where T : EntityBase<T, TId>
{
  /// <summary>
  /// The unique identifier for the entity.
  /// </summary>
  public TId Id { get; set; } = default!;
}

