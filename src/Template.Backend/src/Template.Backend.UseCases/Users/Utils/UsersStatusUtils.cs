using Template.Backend.Domain.Aggregates.Users;
using Template.Backend.UseCases.Users.Dtos;

namespace Template.Backend.UseCases.Users.Utils;

public static class UsersStatusUtils
{
  public static UserMembershipStatus ComputeStatus(bool deactivated) =>
    deactivated ? UserMembershipStatus.Deactivated : UserMembershipStatus.Active;

  public static UserMembershipStatus ComputeStatus(User user) =>
    ComputeStatus(user.Deactivated);
}
