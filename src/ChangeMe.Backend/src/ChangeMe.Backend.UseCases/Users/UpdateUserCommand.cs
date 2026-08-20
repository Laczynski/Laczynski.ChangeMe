using ChangeMe.Backend.Domain.Aggregates.Roles;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Users.Dtos;
using ChangeMe.Backend.UseCases.Users.Utils;

namespace ChangeMe.Backend.UseCases.Users;

public sealed record UpdateUserCommand(
  Guid Id,
  long Version,
  string FirstName,
  string LastName,
  string Email,
  IReadOnlyList<Guid>? RoleIds,
  bool? Deactivated) : ICommand<UserDetailsDto>;

public class UpdateUserHandler(
  IMediator mediator,
  ApplicationDbContext context,
  IUserAccessor userAccessor) : ICommandHandler<UpdateUserCommand, UserDetailsDto>
{
  public async ValueTask<Result<UserDetailsDto>> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
  {
    var user = await context.Users.Include(x => x.Roles).FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
    if (user is null)
      return Result<UserDetailsDto>.NotFound();

    var versionCheck = ConcurrencyGuard.CheckExpectedVersion(user, command.Version);
    if (!versionCheck.IsSuccess)
      return versionCheck.Map();

    var normalizedEmail = User.NormalizeEmail(command.Email);
    var emailChanged = !user.NormalizedEmail.Equals(normalizedEmail, StringComparison.Ordinal);

    var profileResult = await TryUpdateProfileAsync(user, command, normalizedEmail, emailChanged, cancellationToken);
    if (!profileResult.IsSuccess)
      return profileResult.Map();

    if (command.RoleIds is not null)
    {
      var roleResult = await TryUpdateRolesAsync(user, command.RoleIds, cancellationToken);
      if (!roleResult.IsSuccess)
        return roleResult.Map();
    }

    if (command.Deactivated.HasValue)
    {
      var deactivationResult = await TryUpdateDeactivationAsync(user, command.Deactivated.Value, cancellationToken);
      if (!deactivationResult.IsSuccess)
        return deactivationResult.Map();
    }

    if (emailChanged)
      await UsersUtils.RevokeAllActiveSessionsAsync(context, user.Id, cancellationToken);

    await context.SaveChangesAsync(cancellationToken);

    return await GetUpdatedUserAsync(user.Id, cancellationToken);
  }

  private async Task<Result> TryUpdateProfileAsync(
    User user,
    UpdateUserCommand command,
    string normalizedEmail,
    bool emailChanged,
    CancellationToken cancellationToken)
  {
    if (emailChanged
        && await UsersUtils.IsProfileEmailTakenAsync(context, normalizedEmail, user.Id, cancellationToken))
      return Result.Conflict(UsersUtils.DuplicateEmailMessage);

    var updateProfileResult = user.UpdateAdminProfile(command.FirstName, command.LastName, command.Email);
    return updateProfileResult.IsSuccess ? Result.Success() : updateProfileResult;
  }

  private async Task<Result> TryUpdateRolesAsync(
    User user,
    IReadOnlyList<Guid> roleIds,
    CancellationToken cancellationToken)
  {
    if (!userAccessor.HasPermission(PermissionCodes.RolesManage))
      return Result.Forbidden(UsersUtils.PermissionDeniedMessage);

    var currentUserId = userAccessor.UserId!.Value;
    if (currentUserId == user.Id)
      return Result.Error(UsersUtils.CannotChangeOwnRolesMessage);

    var distinctRoleIds = roleIds.Distinct().ToList();
    var existingRoleCount = await context.Roles
      .CountAsync(x => distinctRoleIds.Contains(x.Id), cancellationToken);

    if (existingRoleCount != distinctRoleIds.Count)
      return Result.NotFound();

    if (currentUserId == user.Id)
    {
      var administratorRoleId = await context.Roles
        .AsNoTracking()
        .Where(x => x.Name == RoleConstraints.AdministratorRoleName)
        .Select(x => x.Id)
        .FirstOrDefaultAsync(cancellationToken);

      if (administratorRoleId != Guid.Empty
          && user.HasRole(administratorRoleId)
          && !distinctRoleIds.Contains(administratorRoleId))
        return Result.Error(UsersUtils.CannotRemoveOwnAdministratorMessage);
    }

    var roleResult = user.ReplaceRoles(distinctRoleIds);
    return roleResult.IsSuccess ? Result.Success() : roleResult;
  }

  private async Task<Result> TryUpdateDeactivationAsync(
    User user,
    bool deactivated,
    CancellationToken cancellationToken)
  {
    if (!userAccessor.HasPermission(PermissionCodes.UsersDeactivate))
      return Result.Forbidden(UsersUtils.PermissionDeniedMessage);

    if (userAccessor.UserId!.Value == user.Id && deactivated)
      return Result.Error(UsersUtils.CannotDeactivateOwnAccountMessage);

    if (deactivated && user.IsActive)
    {
      var validationResult = await UsersUtils.ValidateCanDeactivateUserAsync(context, user.Id, cancellationToken);
      if (!validationResult.IsSuccess)
        return validationResult;

      user.Deactivate();
      await UsersUtils.RevokeAllActiveSessionsAsync(context, user.Id, cancellationToken);
    }
    else if (!deactivated && !user.IsActive)
    {
      user.Activate();
    }

    return Result.Success();
  }

  private async Task<Result<UserDetailsDto>> GetUpdatedUserAsync(Guid userId, CancellationToken cancellationToken)
  {
    var updatedUserResult = await mediator.Send(new GetUserByIdQuery(userId), cancellationToken);
    if (!updatedUserResult.IsSuccess)
      return updatedUserResult.Map();

    return updatedUserResult;
  }
}
