using DataGrid.Abstractions;
using Template.Backend.Domain.Aggregates.Users;
using Template.Backend.Infrastructure.Auth;
using Template.Backend.UnitTests.Support;
using Template.Backend.UseCases.Users;
using Template.Backend.UseCases.Users.Dtos;

namespace Template.Backend.UnitTests.UseCases.Users;

public sealed class GetUsersHandlerTests
{
  [Fact]
  public async Task Handle_WhenStatusFilterIsSet_ReturnsOnlyMatchingUsers()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    await using var context = UseCasesTestDb.Create(
      nameof(Handle_WhenStatusFilterIsSet_ReturnsOnlyMatchingUsers));

    var passwordHasher = new PasswordHasherAdapter();
    var activeUser = User.Create(
      "Active",
      "User",
      "active@example.com",
      passwordHasher.HashPassword("hash")).Value;
    var deactivatedUser = User.Create(
      "Deactivated",
      "User",
      "deactivated@example.com",
      passwordHasher.HashPassword("hash")).Value;
    deactivatedUser.Deactivate();

    await context.Users.AddRangeAsync(activeUser, deactivatedUser);
    await context.SaveChangesAsync(cancellationToken);

    var handler = new GetUsersHandler(context);
    var result = await handler.Handle(
      new GetUsersQuery
      {
        Grid = new GridQuery
        {
          Take = 10,
          Filter = new FilterCondition
          {
            Field = nameof(UserListItemDto.Status),
            Operator = FilterOperator.Eq,
            Value = UserMembershipStatus.Deactivated
          }
        }
      },
      cancellationToken);

    Assert.True(result.IsSuccess);
    Assert.Single(result.Value.Items);
    Assert.Equal(deactivatedUser.Id, result.Value.Items[0].Id);
    Assert.Equal(UserMembershipStatus.Deactivated, result.Value.Items[0].Status);
  }
}
