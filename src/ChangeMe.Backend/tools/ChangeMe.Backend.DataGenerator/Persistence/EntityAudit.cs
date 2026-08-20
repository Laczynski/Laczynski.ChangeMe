namespace ChangeMe.Backend.DataGenerator.Persistence;

internal static class EntityAudit
{
  public static void Apply(Entity entity, Guid actorId)
  {
    entity.CreatedBy = actorId;
    entity.UpdatedBy = actorId;
  }

  public static void Apply(IEnumerable<Entity> entities, Guid actorId)
  {
    foreach (var entity in entities)
      Apply(entity, actorId);
  }
}
