using ChangeMe.Backend.Domain.Aggregates.Users.Entities;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Users;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
  public void Configure(EntityTypeBuilder<UserRole> builder)
  {
    builder.ToTable("user_roles");

    builder.HasKey(x => new { x.UserId, x.RoleId });

    builder.Property(x => x.UserId)
      .IsRequired();

    builder.Property(x => x.RoleId)
      .IsRequired();

    builder.HasOne(x => x.Role)
      .WithMany()
      .HasForeignKey(x => x.RoleId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}
