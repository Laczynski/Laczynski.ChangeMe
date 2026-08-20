using Bogus;
using ChangeMe.Backend.DataGenerator.Options;
using ChangeMe.Backend.DataGenerator.Persistence;
using ChangeMe.Backend.Domain.Aggregates.Roles;
using ChangeMe.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using DomainUser = global::ChangeMe.Backend.Domain.Aggregates.Users.User;

namespace ChangeMe.Backend.DataGenerator.Generators;

internal sealed class UsersGenerator(
  ApplicationDbContext dbContext,
  IPasswordHasher passwordHasher,
  IOptions<DataGeneratorOptions> options)
{
  public async Task<IReadOnlyList<DomainUser>> GenerateAsync(CancellationToken cancellationToken)
  {
    var config = options.Value;
    var userRole = await dbContext.Roles
      .FirstAsync(r => r.Name == RoleConstraints.UserRoleName, cancellationToken);

    var faker = new Faker { Random = new Randomizer(config.Seed) };
    var passwordHash = passwordHasher.HashPassword(config.DefaultPassword);
    var users = new List<DomainUser>();

    for (var index = 1; index <= config.Users; index++)
    {
      var firstName = faker.Name.FirstName();
      var lastName = faker.Name.LastName();
      var email = $"user{index}@{config.EmailDomain.Trim().ToLowerInvariant()}";

      var createResult = DomainUser.Create(firstName, lastName, email, passwordHash);
      if (!createResult.IsSuccess)
        throw new InvalidOperationException($"Failed to create demo user {email}: {string.Join(", ", createResult.ValidationErrors.Select(e => e.ErrorMessage))}");

      var user = createResult.Value;
      user.AssignRole(userRole.Id);
      EntityAudit.Apply(user, user.Id);

      users.Add(user);
      await dbContext.Users.AddAsync(user, cancellationToken);
    }

    await dbContext.SaveChangesAsync(cancellationToken);
    return users;
  }
}
