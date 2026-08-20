using ChangeMe.Backend.Domain.Aggregates.Issue;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Issues;

public class IssueConfiguration : BaseEntityTypeConfiguration<Issue>
{
  protected override string TableName => "issues";

  public override void Configure(EntityTypeBuilder<Issue> builder)
  {
    base.Configure(builder);

    builder.Property(i => i.Title)
      .IsRequired()
      .HasMaxLength(IssueConstraints.TITLE_MAX_LENGTH);

    builder.Property(i => i.Description)
      .IsRequired()
      .HasMaxLength(IssueConstraints.DESCRIPTION_MAX_LENGTH);

    builder.Property(i => i.Status)
      .IsRequired()
      .HasConversion<string>();

    builder.Property(i => i.Priority)
      .IsRequired()
      .HasConversion<string>();

    builder.Property(i => i.AssignedToUserId)
      .HasColumnType("uuid");

    builder.Property(i => i.LastActivityAt)
      .IsRequired();

    builder.HasMany(i => i.AcceptanceCriteria)
      .WithOne()
      .HasForeignKey(ic => ic.IssueId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasMany(i => i.Comments)
      .WithOne()
      .HasForeignKey(ic => ic.IssueId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasMany(i => i.HistoryEntries)
      .WithOne()
      .HasForeignKey(ih => ih.IssueId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasMany(i => i.Watchers)
      .WithOne()
      .HasForeignKey(iw => iw.IssueId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}
