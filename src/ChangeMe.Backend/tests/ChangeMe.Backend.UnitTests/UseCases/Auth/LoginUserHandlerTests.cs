using Ardalis.Result;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UnitTests.Support;
using ChangeMe.Backend.UseCases.Auth;
using ChangeMe.Backend.UseCases.Auth.Utils;
using Microsoft.AspNetCore.Http;

namespace ChangeMe.Backend.UnitTests.UseCases.Auth;

public sealed class LoginUserHandlerTests
{
  [Fact]
  public async Task Handle_WhenCredentialsAreValid_ReturnsAuthSession()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    await using var context = UseCasesTestDb.Create(nameof(Handle_WhenCredentialsAreValid_ReturnsAuthSession));

    const string password = "StrongPass123!";
    var passwordHasher = new PasswordHasherAdapter();
    var user = User.Create(
      "Test",
      "User",
      "user@example.com",
      passwordHasher.HashPassword(password)).Value;

    await context.Users.AddAsync(user, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    var authOptions = TestAuthOptions.Create();
    var handler = CreateHandler(context, passwordHasher, authOptions);

    var result = await handler.Handle(
      new LoginUserCommand("user@example.com", password),
      cancellationToken);

    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value.AuthSession.Token);
    Assert.Equal(user.Id, result.Value.AuthSession.UserId);
    Assert.Equal(user.Email, result.Value.AuthSession.Email);
  }

  [Fact]
  public async Task Handle_WhenPasswordIsInvalid_ReturnsUnauthorized()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    await using var context = UseCasesTestDb.Create(nameof(Handle_WhenPasswordIsInvalid_ReturnsUnauthorized));

    var passwordHasher = new PasswordHasherAdapter();
    var user = User.Create(
      "Test",
      "User",
      "user@example.com",
      passwordHasher.HashPassword("StrongPass123!")).Value;

    await context.Users.AddAsync(user, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    var handler = CreateHandler(context, passwordHasher, TestAuthOptions.Create());

    var result = await handler.Handle(
      new LoginUserCommand("user@example.com", "WrongPass123!"),
      cancellationToken);

    Assert.Equal(ResultStatus.Unauthorized, result.Status);
    Assert.Contains(AuthSessionUtils.InvalidCredentialsMessage, result.Errors.First());
  }

  [Fact]
  public async Task Handle_WhenUserIsDeactivated_ReturnsUnauthorized()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    await using var context = UseCasesTestDb.Create(nameof(Handle_WhenUserIsDeactivated_ReturnsUnauthorized));

    const string password = "StrongPass123!";
    var passwordHasher = new PasswordHasherAdapter();
    var user = User.Create(
      "Test",
      "User",
      "deactivated@example.com",
      passwordHasher.HashPassword(password)).Value;
    user.Deactivate();

    await context.Users.AddAsync(user, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    var handler = CreateHandler(context, passwordHasher, TestAuthOptions.Create());

    var result = await handler.Handle(
      new LoginUserCommand("deactivated@example.com", password),
      cancellationToken);

    Assert.Equal(ResultStatus.Unauthorized, result.Status);
    Assert.Contains(AuthSessionUtils.DeactivatedAccountMessage, result.Errors.First());
  }

  private static LoginUserHandler CreateHandler(
    ChangeMe.Backend.Infrastructure.Persistence.ApplicationDbContext context,
    PasswordHasherAdapter passwordHasher,
    Microsoft.Extensions.Options.IOptions<AuthOptions> authOptions) =>
    new(
      context,
      passwordHasher,
      new JwtTokenGenerator(authOptions),
      new SessionLifetimeService(authOptions),
      new HttpContextAccessor { HttpContext = new DefaultHttpContext() });
}
