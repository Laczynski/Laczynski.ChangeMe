using ChangeMe.Backend.UseCases.Users.Dtos;

namespace ChangeMe.Backend.UseCases.Users;

public sealed record ActivateUserCommand(Guid Id) : ICommand<UserDetailsDto>;

public class ActivateUserHandler(
  IMediator mediator,
  ApplicationDbContext context) : ICommandHandler<ActivateUserCommand, UserDetailsDto>
{
  public async ValueTask<Result<UserDetailsDto>> Handle(ActivateUserCommand command, CancellationToken cancellationToken)
  {
    var user = await context.Users.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
    if (user is null)
      return Result<UserDetailsDto>.NotFound();

    if (!user.IsActive)
    {
      user.Activate();
      await context.SaveChangesAsync(cancellationToken);
    }

    var userResult = await mediator.Send(new GetUserByIdQuery(user.Id), cancellationToken);
    if (!userResult.IsSuccess)
      return userResult.Map();

    return userResult;
  }
}
