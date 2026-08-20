using ChangeMe.Backend.Domain.Aggregates.Issue.Entities;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Issues;

public class IssueAcceptanceCriterionConfiguration : BaseEntityTypeConfiguration<IssueAcceptanceCriterion>
{
  protected override string TableName => "issue_acceptance_criteria";

  public override void Configure(EntityTypeBuilder<IssueAcceptanceCriterion> builder)
  {
    base.Configure(builder);

    builder.Property(ic => ic.IssueId)
      .IsRequired()
      .HasColumnType("uuid");

    builder.Property(ic => ic.Content)
      .IsRequired()
      .HasMaxLength(IssueAcceptanceCriterionConstraints.CONTENT_MAX_LENGTH);
  }
}
