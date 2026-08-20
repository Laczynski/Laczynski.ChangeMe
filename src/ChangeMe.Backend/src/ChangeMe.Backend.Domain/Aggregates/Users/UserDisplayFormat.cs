using System.Linq.Expressions;

namespace ChangeMe.Backend.Domain.Aggregates.Users;

public static class UserDisplayFormat
{
  public static Expression<Func<User, string>> DisplayLabelExpression { get; } = user =>
    user.FirstName == "" && user.LastName == ""
      ? user.Email
      : (user.FirstName + " " + user.LastName).Trim() + " (" + user.Email + ")";

  public static string DisplayLabel(string firstName, string lastName, string email)
  {
    var first = firstName.Trim();
    var last = lastName.Trim();
    if (first.Length == 0 && last.Length == 0)
      return email;

    return $"{string.Join(' ', new[] { first, last }.Where(part => part.Length > 0))} ({email})";
  }
}
