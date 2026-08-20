using Ardalis.Result;

namespace ChangeMe.Backend.UnitTests.Domain.Common;

public sealed class ConcurrencyGuardTests
{
  private sealed class TestEntity : Entity;

  [Fact]
  public void CheckExpectedVersion_WhenVersionsMatch_ShouldReturnSuccess()
  {
    var entity = new TestEntity { Version = 3 };

    var result = ConcurrencyGuard.CheckExpectedVersion(entity, 3);

    Assert.True(result.IsSuccess);
  }

  [Fact]
  public void CheckExpectedVersion_WhenVersionsDiffer_ShouldReturnConflict()
  {
    var entity = new TestEntity { Version = 3 };

    var result = ConcurrencyGuard.CheckExpectedVersion(entity, 2);

    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.Conflict, result.Status);
    Assert.Contains(ConcurrencyMessages.StaleVersion, result.Errors);
  }
}
