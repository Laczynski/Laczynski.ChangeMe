using ChangeMe.Backend.Domain.Aggregates.Notifications;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Notifications;

public class NotificationConfiguration : BaseEntityTypeConfiguration<Notification>
{
  protected override string TableName => "notifications";

  public override void Configure(EntityTypeBuilder<Notification> builder)
  {
    base.Configure(builder);

    builder.Property(n => n.RecipientUserId)
      .IsRequired()
      .HasColumnType("uuid");

    builder.Property(n => n.IssueId)
      .IsRequired()
      .HasColumnType("uuid");

    builder.Property(n => n.IssueHistoryEntryId)
      .IsRequired()
      .HasColumnType("uuid");

    builder.Property(n => n.EventType)
      .IsRequired()
      .HasConversion<string>();

    builder.Property(n => n.IssueTitle)
      .IsRequired()
      .HasMaxLength(NotificationConstraints.ISSUE_TITLE_MAX_LENGTH);

    builder.Property(n => n.Message)
      .IsRequired()
      .HasMaxLength(NotificationConstraints.MESSAGE_MAX_LENGTH);

    builder.Property(n => n.Link)
      .IsRequired()
      .HasMaxLength(NotificationConstraints.LINK_MAX_LENGTH);

    builder.Property(n => n.IsRead)
      .IsRequired();

    builder.HasIndex(n => new { n.RecipientUserId, n.IssueHistoryEntryId })
      .IsUnique();

    builder.HasIndex(n => new { n.RecipientUserId, n.IsRead });
  }
}
