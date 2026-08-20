using Ardalis.Result;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UnitTests.Support;
using ChangeMe.Backend.UseCases.Roles;

namespace ChangeMe.Backend.UnitTests.UseCases.Roles;

public sealed class CreateRoleHandlerTests
{
  [Fact]
  public async Task Handle_WhenValid_ShouldReturnCreated()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    await using var context = UseCasesTestDb.Create(nameof(Handle_WhenValid_ShouldReturnCreated));
    await UseCasesTestDb.SeedSystemRolesAsync(context);

    var handler = new CreateRoleHandler(
      new GetRoleByIdDispatchingTestMediator(context),
      context);

    var result = await handler.Handle(
      new CreateRoleCommand("Custom Role", "Description", [PermissionCodes.UsersView]),
      cancellationToken);

    Assert.Equal(ResultStatus.Created, result.Status);
    Assert.NotNull(result.Value);
    Assert.Equal("Custom Role", result.Value.Name);
  }
}
