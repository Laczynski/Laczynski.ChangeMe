using ChangeMe.Backend.Domain.Aggregates.Sessions;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using ChangeMe.Backend.UseCases.Auth.Utils;
using Microsoft.AspNetCore.Http;

namespace ChangeMe.Backend.UseCases.Auth;

public sealed record LoginUserCommand(
  string Email,
  string Password) : ICommand<LoginResponseDto>;

public class LoginUserHandler(
  ApplicationDbContext context,
  IPasswordHasher passwordHasher,
  IJwtTokenGenerator jwtTokenGenerator,
  ISessionLifetimeService sessionLifetime,
  IHttpContextAccessor httpContextAccessor) : ICommandHandler<LoginUserCommand, LoginResponseDto>
{
  public async ValueTask<Result<LoginResponseDto>> Handle(LoginUserCommand command, CancellationToken cancellationToken)
  {
    var normalizedEmail = User.NormalizeEmail(command.Email);
    var user = await context.Users
      .FirstOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);
    if (user is null)
      return Result<LoginResponseDto>.Unauthorized(AuthSessionUtils.InvalidCredentialsMessage);

    if (!user.IsActive)
      return Result<LoginResponseDto>.Unauthorized(AuthSessionUtils.DeactivatedAccountMessage);

    if (!passwordHasher.VerifyPassword(user.PasswordHash, command.Password))
      return Result<LoginResponseDto>.Unauthorized(AuthSessionUtils.InvalidCredentialsMessage);

    var sessionResult = await AuthSessionFactory.CreateSessionAsync(
      context,
      sessionLifetime,
      httpContextAccessor,
      user,
      cancellationToken,
      SignInMethods.Password);
    if (!sessionResult.IsSuccess)
      return Result<LoginResponseDto>.Invalid(sessionResult.ValidationErrors);

    await context.SaveChangesAsync(cancellationToken);

    var authResponse = await AuthSessionUtils.CreateAuthResponseAsync(
      context,
      jwtTokenGenerator,
      user,
      sessionResult.Value.Session,
      sessionResult.Value.RefreshToken,
      cancellationToken);

    if (!authResponse.IsSuccess)
      return authResponse.Map();

    return Result.Success(new LoginResponseDto(authResponse.Value));
  }
}
