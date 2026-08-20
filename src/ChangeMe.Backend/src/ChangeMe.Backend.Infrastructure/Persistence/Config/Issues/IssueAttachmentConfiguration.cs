using ChangeMe.Backend.Domain.Aggregates.Issue;
using ChangeMe.Backend.Domain.Aggregates.Issue.Entities;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Issues;

public class IssueAttachmentConfiguration : IEntityTypeConfiguration<IssueAttachment>
{
  public void Configure(EntityTypeBuilder<IssueAttachment> builder)
  {
    builder.HasOne<Issue>()
      .WithMany(i => i.Attachments)
      .HasForeignKey(a => a.OwnerId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}
