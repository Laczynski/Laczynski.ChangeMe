using ChangeMe.Backend.UseCases.Issues.Dtos;

namespace ChangeMe.Backend.UseCases.Issues;

public sealed record GetAssignableUsersQuery() : IQuery<List<IssueAssignableUserDto>>;

public class GetAssignableUsersHandler(ApplicationDbContext context)
  : IQueryHandler<GetAssignableUsersQuery, List<IssueAssignableUserDto>>
{
  public async ValueTask<Result<List<IssueAssignableUserDto>>> Handle(
    GetAssignableUsersQuery query,
    CancellationToken cancellationToken)
  {
    var users = await context.Users
      .AsNoTracking()
      .Where(u => !u.Deactivated)
      .OrderBy(u => u.LastName)
      .ThenBy(u => u.FirstName)
      .ToListAsync(cancellationToken);

    var assignableUsers = users
      .Select(u => new IssueAssignableUserDto
      {
        Id = u.Id,
        DisplayLabel = u.DisplayLabel,
      })
      .ToList();

    return Result.Success(assignableUsers);
  }
}
