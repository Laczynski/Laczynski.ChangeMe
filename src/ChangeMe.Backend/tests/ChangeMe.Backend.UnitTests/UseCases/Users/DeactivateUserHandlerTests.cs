using Ardalis.Result;
using ChangeMe.Backend.Domain.Aggregates.Roles;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UnitTests.Support;
using ChangeMe.Backend.UseCases.Users;
using ChangeMe.Backend.UseCases.Users.Utils;
using Microsoft.EntityFrameworkCore;

namespace ChangeMe.Backend.UnitTests.UseCases.Users;

public sealed class DeactivateUserHandlerTests
{
  [Fact]
  public async Task Handle_WhenLastActiveAdministrator_ShouldReturnError()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    await using var context = UseCasesTestDb.Create(nameof(Handle_WhenLastActiveAdministrator_ShouldReturnError));
    await UseCasesTestDb.SeedSystemRolesAsync(context);

    var administratorRoleId = await context.Roles
      .Where(x => x.Name == RoleConstraints.AdministratorRoleName)
      .Select(x => x.Id)
      .SingleAsync(cancellationToken);

    var passwordHasher = new PasswordHasherAdapter();
    var admin = User.Create(
      "Admin",
      "User",
      "only-admin@example.com",
      passwordHasher.HashPassword("StrongPass123!")).Value;
    admin.AssignRole(administratorRoleId);

    var actingAdminId = Guid.NewGuid();
    await context.Users.AddAsync(admin, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    var handler = new DeactivateUserHandler(
      new GetUserByIdDispatchingTestMediator(context),
      context,
      new FakeUserAccessor { UserId = actingAdminId });

    var result = await handler.Handle(new DeactivateUserCommand(admin.Id), cancellationToken);

    Assert.Equal(ResultStatus.Error, result.Status);
    Assert.Contains(UsersUtils.CannotDeactivateLastAdministratorMessage, result.Errors.First());
    Assert.True((await context.Users.FindAsync([admin.Id], cancellationToken))!.IsActive);
  }

  [Fact]
  public async Task Handle_WhenAnotherActiveAdministratorExists_ShouldDeactivateUser()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    await using var context = UseCasesTestDb.Create(nameof(Handle_WhenAnotherActiveAdministratorExists_ShouldDeactivateUser));
    await UseCasesTestDb.SeedSystemRolesAsync(context);

    var administratorRoleId = await context.Roles
      .Where(x => x.Name == RoleConstraints.AdministratorRoleName)
      .Select(x => x.Id)
      .SingleAsync(cancellationToken);

    var passwordHasher = new PasswordHasherAdapter();
    var targetAdmin = User.Create(
      "Target",
      "Admin",
      "target-admin@example.com",
      passwordHasher.HashPassword("StrongPass123!")).Value;
    targetAdmin.AssignRole(administratorRoleId);

    var otherAdmin = User.Create(
      "Other",
      "Admin",
      "other-admin@example.com",
      passwordHasher.HashPassword("StrongPass123!")).Value;
    otherAdmin.AssignRole(administratorRoleId);

    await context.Users.AddRangeAsync(targetAdmin, otherAdmin);
    await context.SaveChangesAsync(cancellationToken);

    var handler = new DeactivateUserHandler(
      new GetUserByIdDispatchingTestMediator(context),
      context,
      new FakeUserAccessor { UserId = otherAdmin.Id });

    var result = await handler.Handle(new DeactivateUserCommand(targetAdmin.Id), cancellationToken);

    Assert.True(result.IsSuccess);
    Assert.False((await context.Users.FindAsync([targetAdmin.Id], cancellationToken))!.IsActive);
  }

  [Fact]
  public async Task Handle_WhenUserIsNotAdministrator_ShouldDeactivateUser()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    await using var context = UseCasesTestDb.Create(nameof(Handle_WhenUserIsNotAdministrator_ShouldDeactivateUser));
    await UseCasesTestDb.SeedSystemRolesAsync(context);

    var userRoleId = await context.Roles
      .Where(x => x.Name == RoleConstraints.UserRoleName)
      .Select(x => x.Id)
      .SingleAsync(cancellationToken);

    var passwordHasher = new PasswordHasherAdapter();
    var user = User.Create(
      "Regular",
      "User",
      "regular-user@example.com",
      passwordHasher.HashPassword("StrongPass123!")).Value;
    user.AssignRole(userRoleId);

    await context.Users.AddAsync(user, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    var handler = new DeactivateUserHandler(
      new GetUserByIdDispatchingTestMediator(context),
      context,
      new FakeUserAccessor { UserId = Guid.NewGuid() });

    var result = await handler.Handle(new DeactivateUserCommand(user.Id), cancellationToken);

    Assert.True(result.IsSuccess);
    Assert.False((await context.Users.FindAsync([user.Id], cancellationToken))!.IsActive);
  }
}
