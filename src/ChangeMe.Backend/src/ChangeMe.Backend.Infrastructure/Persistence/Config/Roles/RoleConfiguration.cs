using ChangeMe.Backend.Domain.Aggregates.Roles;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Roles;

public class RoleConfiguration : BaseEntityTypeConfiguration<Role>
{
  protected override string TableName => "roles";

  public override void Configure(EntityTypeBuilder<Role> builder)
  {
    base.Configure(builder);

    builder.Property(x => x.Name)
      .IsRequired()
      .HasMaxLength(RoleConstraints.NAME_MAX_LENGTH);

    builder.Property(x => x.Description)
      .HasMaxLength(RoleConstraints.DESCRIPTION_MAX_LENGTH);

    builder.Property(x => x.IsSystem)
      .IsRequired();

    builder.HasIndex(x => x.Name)
      .IsUnique();

    builder.HasMany(x => x.Permissions)
      .WithOne(x => x.Role)
      .HasForeignKey(x => x.RoleId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Navigation(x => x.Permissions)
      .UsePropertyAccessMode(PropertyAccessMode.Field);
  }
}
