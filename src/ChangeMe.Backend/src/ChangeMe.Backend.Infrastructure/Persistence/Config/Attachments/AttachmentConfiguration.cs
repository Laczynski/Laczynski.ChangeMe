using ChangeMe.Backend.Domain.Aggregates.Issue.Entities;
using ChangeMe.Backend.Domain.Common.Attachments;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Attachments;

public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
  public void Configure(EntityTypeBuilder<Attachment> builder)
  {
    builder.ToTable("attachments");
    builder.HasDiscriminator(a => a.Type)
      .HasValue<IssueAttachment>(AttachmentType.Issue);

    builder.Property(a => a.Type)
      .IsRequired()
      .HasConversion<string>()
      .HasMaxLength(AttachmentConstraints.TYPE_MAX_LENGTH);

    builder.HasKey(e => e.Id);

    builder.Property(e => e.Id)
      .ValueGeneratedNever();

    builder.Property(e => e.CreatedAt)
      .IsRequired();

    builder.Property(e => e.UpdatedAt);

    builder.Property(e => e.IsDeleted)
      .HasDefaultValue(false);

    builder.Property(e => e.CreatedBy)
      .IsRequired();

    builder.Property(e => e.UpdatedBy);

    builder.HasIndex(e => e.Id);

    builder.HasQueryFilter(e => !e.IsDeleted);

    builder.Property(a => a.StorageContainer)
      .IsRequired()
      .HasMaxLength(AttachmentConstraints.STORAGE_CONTAINER_MAX_LENGTH);

    builder.Property(a => a.OwnerId)
      .IsRequired()
      .HasColumnType("uuid");

    builder.Property(a => a.OriginalFileName)
      .IsRequired()
      .HasMaxLength(AttachmentConstraints.ORIGINAL_FILE_NAME_MAX_LENGTH);

    builder.Property(a => a.ContentType)
      .IsRequired()
      .HasMaxLength(AttachmentConstraints.CONTENT_TYPE_MAX_LENGTH);

    builder.Property(a => a.SizeBytes)
      .IsRequired();

    builder.Property(a => a.StorageKey)
      .IsRequired()
      .HasMaxLength(AttachmentConstraints.STORAGE_KEY_MAX_LENGTH);

    builder.HasIndex(a => a.OwnerId);
    builder.HasIndex(a => new { a.StorageContainer, a.OwnerId, a.StorageKey }).IsUnique();
  }
}
