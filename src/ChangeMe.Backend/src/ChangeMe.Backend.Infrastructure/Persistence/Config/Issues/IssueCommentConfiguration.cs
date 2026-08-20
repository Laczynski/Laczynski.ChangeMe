using ChangeMe.Backend.Domain.Aggregates.Issue.Entities;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Issues;

public class IssueCommentConfiguration : BaseEntityTypeConfiguration<IssueComment>
{
  protected override string TableName => "issue_comments";

  public override void Configure(EntityTypeBuilder<IssueComment> builder)
  {
    base.Configure(builder);

    builder.Property(ic => ic.IssueId)
      .IsRequired()
      .HasColumnType("uuid");

    builder.Property(ic => ic.Content)
      .IsRequired()
      .HasMaxLength(IssueCommentConstraints.CONTENT_MAX_LENGTH);

    builder.HasIndex(ic => ic.IssueId);
  }
}
