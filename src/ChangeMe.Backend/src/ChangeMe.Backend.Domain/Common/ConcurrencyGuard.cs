namespace ChangeMe.Backend.Domain.Common;

public static class ConcurrencyGuard
{
  public static Result CheckExpectedVersion(Entity entity, long expectedVersion) =>
    entity.Version == expectedVersion
      ? Result.Success()
      : Result.Conflict(ConcurrencyMessages.StaleVersion);
}
