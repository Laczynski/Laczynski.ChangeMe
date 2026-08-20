using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.UseCases.Users.Dtos;
using ChangeMe.Backend.UseCases.Users.Utils;

namespace ChangeMe.Backend.UseCases.Users;

public sealed record CreateUserCommand(
  string FirstName,
  string LastName,
  string Email,
  string Password,
  IReadOnlyList<Guid> RoleIds) : ICommand<UserDetailsDto>;

public class CreateUserHandler(
  IMediator mediator,
  ApplicationDbContext context,
  IPasswordHasher passwordHasher) : ICommandHandler<CreateUserCommand, UserDetailsDto>
{
  public async ValueTask<Result<UserDetailsDto>> Handle(CreateUserCommand command, CancellationToken cancellationToken)
  {
    var normalizedEmail = User.NormalizeEmail(command.Email);
    var emailExists = await context.Users.AnyAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);
    if (emailExists)
      return Result<UserDetailsDto>.Conflict(UsersUtils.DuplicateEmailMessage);

    var passwordHash = passwordHasher.HashPassword(command.Password);
    var createUserResult = User.Create(command.FirstName, command.LastName, command.Email, passwordHash);
    if (!createUserResult.IsSuccess)
      return createUserResult.Map();

    var user = createUserResult.Value;

    var distinctRoleIds = command.RoleIds.Distinct().ToList();
    var existingRoleCount = await context.Roles
      .CountAsync(x => distinctRoleIds.Contains(x.Id), cancellationToken);

    if (existingRoleCount != distinctRoleIds.Count)
      return Result<UserDetailsDto>.NotFound();

    var roleResult = user.ReplaceRoles(distinctRoleIds);
    if (!roleResult.IsSuccess)
      return roleResult.Map();

    await context.Users.AddAsync(user, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    var createdUserResult = await mediator.Send(new GetUserByIdQuery(user.Id), cancellationToken);
    if (!createdUserResult.IsSuccess)
      return createdUserResult.Map();

    return Result.Created(createdUserResult.Value, $"/users/{createdUserResult.Value.Id}");
  }
}
