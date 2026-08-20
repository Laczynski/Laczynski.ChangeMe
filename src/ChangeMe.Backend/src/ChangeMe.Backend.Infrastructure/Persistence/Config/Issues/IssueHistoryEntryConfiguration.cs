using ChangeMe.Backend.Domain.Aggregates.Issue.Entities;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Issues;

public class IssueHistoryEntryConfiguration : BaseEntityTypeConfiguration<IssueHistoryEntry>
{
  protected override string TableName => "issue_history_entries";

  public override void Configure(EntityTypeBuilder<IssueHistoryEntry> builder)
  {
    base.Configure(builder);

    builder.Property(i => i.IssueId)
      .IsRequired()
      .HasColumnType("uuid");

    builder.Property(i => i.ActorUserId)
      .IsRequired()
      .HasColumnType("uuid");

    builder.Property(i => i.EventType)
      .IsRequired()
      .HasConversion<string>();

    builder.Property(i => i.Summary)
      .IsRequired()
      .HasMaxLength(IssueHistoryConstraints.SUMMARY_MAX_LENGTH);

    builder.Property(i => i.PreviousValue)
      .HasMaxLength(IssueHistoryConstraints.VALUE_MAX_LENGTH);

    builder.Property(i => i.CurrentValue)
      .HasMaxLength(IssueHistoryConstraints.VALUE_MAX_LENGTH);

    builder.Property(i => i.RelatedCommentId)
      .HasColumnType("uuid");

    builder.HasIndex(i => i.IssueId);
  }
}
