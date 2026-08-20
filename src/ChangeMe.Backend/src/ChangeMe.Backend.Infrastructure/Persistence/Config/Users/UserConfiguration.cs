using ChangeMe.Backend.Domain.Aggregates.Users;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Users;

public class UserConfiguration : BaseEntityTypeConfiguration<User>
{
  protected override string TableName => "users";

  public override void Configure(EntityTypeBuilder<User> builder)
  {
    base.Configure(builder);

    builder.Property(x => x.FirstName)
      .IsRequired()
      .HasMaxLength(UserConstraints.NAME_MAX_LENGTH);

    builder.Property(x => x.LastName)
      .IsRequired()
      .HasMaxLength(UserConstraints.NAME_MAX_LENGTH);

    builder.Property(x => x.Email)
      .IsRequired()
      .HasMaxLength(UserConstraints.EMAIL_MAX_LENGTH);

    builder.Property(x => x.NormalizedEmail)
      .IsRequired()
      .HasMaxLength(UserConstraints.EMAIL_MAX_LENGTH);

    builder.Property(x => x.PasswordHash)
      .IsRequired()
      .HasMaxLength(512);

    builder.Property(x => x.Deactivated)
      .IsRequired();

    builder.Property(x => x.DeactivatedAt);

    builder.HasIndex(x => x.NormalizedEmail)
      .IsUnique();

    builder.HasMany(x => x.Roles)
      .WithOne(x => x.User)
      .HasForeignKey(x => x.UserId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Navigation(x => x.Roles)
      .UsePropertyAccessMode(PropertyAccessMode.Field);
  }
}
