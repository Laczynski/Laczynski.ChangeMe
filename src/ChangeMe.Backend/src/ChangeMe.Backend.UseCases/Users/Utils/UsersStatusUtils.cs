using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.UseCases.Users.Dtos;

namespace ChangeMe.Backend.UseCases.Users.Utils;

public static class UsersStatusUtils
{
  public static UserMembershipStatus ComputeStatus(bool deactivated) =>
    deactivated ? UserMembershipStatus.Deactivated : UserMembershipStatus.Active;

  public static UserMembershipStatus ComputeStatus(User user) =>
    ComputeStatus(user.Deactivated);
}
