using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UnitTests.Support;
using ChangeMe.Backend.UseCases.Issues;

namespace ChangeMe.Backend.UnitTests.UseCases.Issues;

public sealed class GetAssignableUsersHandlerTests
{
  [Fact]
  public async Task Handle_ReturnsActiveUsersOrderedByName()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    await using var context = UseCasesTestDb.Create(nameof(Handle_ReturnsActiveUsersOrderedByName));

    var passwordHasher = new PasswordHasherAdapter();
    var activeUser = User.Create(
      "Active",
      "User",
      "active@example.com",
      passwordHasher.HashPassword("StrongPass123!")).Value;
    var deactivatedUser = User.Create(
      "Deactivated",
      "User",
      "deactivated@example.com",
      passwordHasher.HashPassword("StrongPass123!")).Value;
    deactivatedUser.Deactivate();

    await context.Users.AddRangeAsync(activeUser, deactivatedUser);
    await context.SaveChangesAsync(cancellationToken);

    var handler = new GetAssignableUsersHandler(context);
    var result = await handler.Handle(new GetAssignableUsersQuery(), cancellationToken);

    Assert.True(result.IsSuccess);
    Assert.Contains(result.Value, x => x.Id == activeUser.Id);
    Assert.DoesNotContain(result.Value, x => x.Id == deactivatedUser.Id);
    Assert.Equal(
      "Active User (active@example.com)",
      result.Value.Single(x => x.Id == activeUser.Id).DisplayLabel);
  }
}
