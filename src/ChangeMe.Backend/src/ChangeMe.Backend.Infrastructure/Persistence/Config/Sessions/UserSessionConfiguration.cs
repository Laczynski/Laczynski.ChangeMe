using ChangeMe.Backend.Domain.Aggregates.Sessions;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Sessions;

public class UserSessionConfiguration : BaseEntityTypeConfiguration<UserSession>
{
  protected override string TableName => "user_sessions";

  public override void Configure(EntityTypeBuilder<UserSession> builder)
  {
    base.Configure(builder);

    builder.Property(x => x.UserId)
      .IsRequired();

    builder.Property(x => x.SignedInAt)
      .IsRequired();

    builder.Property(x => x.LastActivityAt)
      .IsRequired();

    builder.Property(x => x.DeviceBrowserLabel)
      .IsRequired()
      .HasMaxLength(SessionConstraints.DEVICE_LABEL_MAX_LENGTH);

    builder.Property(x => x.SignInMethod)
      .IsRequired()
      .HasMaxLength(SessionConstraints.SIGN_IN_METHOD_MAX_LENGTH);

    builder.Property(x => x.IpAddress)
      .HasMaxLength(SessionConstraints.IP_ADDRESS_MAX_LENGTH);

    builder.Property(x => x.RefreshTokenHash)
      .IsRequired()
      .HasMaxLength(128);

    builder.Property(x => x.RefreshTokenExpiresAtUtc)
      .IsRequired();

    builder.Property(x => x.RevokedAt);

    builder.HasIndex(x => x.UserId);
    builder.HasIndex(x => new { x.UserId, x.RevokedAt });
  }
}
