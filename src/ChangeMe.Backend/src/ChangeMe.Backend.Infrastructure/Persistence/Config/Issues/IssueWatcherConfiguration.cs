using ChangeMe.Backend.Domain.Aggregates.Issue.Entities;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Issues;

public class IssueWatcherConfiguration : BaseEntityTypeConfiguration<IssueWatcher>
{
  protected override string TableName => "issue_watchers";

  public override void Configure(EntityTypeBuilder<IssueWatcher> builder)
  {
    base.Configure(builder);

    builder.Property(iw => iw.IssueId)
      .IsRequired()
      .HasColumnType("uuid");

    builder.Property(iw => iw.UserId)
      .IsRequired()
      .HasColumnType("uuid");

    builder.HasIndex(iw => new { iw.IssueId, iw.UserId })
      .IsUnique();
  }
}
