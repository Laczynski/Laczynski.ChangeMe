using ChangeMe.Backend.UseCases.Auth.Dtos;

namespace ChangeMe.Backend.UseCases.Auth;

public sealed record UpdateMyAccountCommand(
  long Version,
  string FirstName,
  string LastName) : ICommand<MyAccountDto>;

public class UpdateMyAccountHandler(
  IMediator mediator,
  ApplicationDbContext context,
  IUserAccessor userAccessor) : ICommandHandler<UpdateMyAccountCommand, MyAccountDto>
{
  public async ValueTask<Result<MyAccountDto>> Handle(UpdateMyAccountCommand command, CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid userId)
      return Result<MyAccountDto>.Unauthorized();

    var user = await context.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
    if (user is null || !user.IsActive)
      return Result<MyAccountDto>.Unauthorized();

    var versionCheck = ConcurrencyGuard.CheckExpectedVersion(user, command.Version);
    if (!versionCheck.IsSuccess)
      return versionCheck.Map();

    var updateResult = user.UpdateProfile(command.FirstName, command.LastName);
    if (!updateResult.IsSuccess)
      return updateResult.Map();

    await context.SaveChangesAsync(cancellationToken);

    var accountResult = await mediator.Send(new GetMyAccountQuery(), cancellationToken);
    if (!accountResult.IsSuccess)
      return accountResult.Map();

    return accountResult;
  }
}
