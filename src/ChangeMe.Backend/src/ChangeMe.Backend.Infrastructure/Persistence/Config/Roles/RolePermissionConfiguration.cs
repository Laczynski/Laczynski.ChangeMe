using ChangeMe.Backend.Domain.Aggregates.Roles.Entities;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Roles;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
  public void Configure(EntityTypeBuilder<RolePermission> builder)
  {
    builder.ToTable("role_permissions");

    builder.HasKey(x => new { x.RoleId, x.PermissionCode });

    builder.Property(x => x.RoleId)
      .IsRequired();

    builder.Property(x => x.PermissionCode)
      .HasMaxLength(64)
      .IsRequired();
  }
}
