using ChangeMe.Backend.Domain.Aggregates.Roles;
using ChangeMe.Backend.UnitTests.Support;
using ChangeMe.Backend.UseCases.Roles;
using DataGrid.Abstractions;

namespace ChangeMe.Backend.UnitTests.UseCases.Roles;

public sealed class GetRolesHandlerSearchTests
{
  [Fact]
  public async Task Handle_WhenSearchMatchesDescription_ReturnsMatchingRole()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    await using var context = UseCasesTestDb.Create(nameof(Handle_WhenSearchMatchesDescription_ReturnsMatchingRole));
    await UseCasesTestDb.SeedSystemRolesAsync(context);

    var marker = $"search-marker-{Guid.NewGuid():N}";
    var customRole = Role.Create("Custom", marker).Value;
    await context.Roles.AddAsync(customRole, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    var handler = new GetRolesHandler(context);
    var result = await handler.Handle(
      new GetRolesQuery
      {
        Grid = new GridQuery { Take = 10, Search = marker }
      },
      cancellationToken);

    Assert.True(result.IsSuccess);
    Assert.Contains(result.Value.Items, r => r.Description == marker);
  }
}
