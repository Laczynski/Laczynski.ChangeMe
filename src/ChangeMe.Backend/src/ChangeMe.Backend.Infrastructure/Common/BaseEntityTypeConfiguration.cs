namespace ChangeMe.Backend.Infrastructure.Common;

public abstract class BaseEntityTypeConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : Entity
{

  protected abstract string TableName { get; }

  public virtual void Configure(EntityTypeBuilder<TEntity> builder)
  {
    builder.ToTable(TableName);

    builder.HasKey(e => e.Id);

    builder.Property(e => e.Id)
      .ValueGeneratedNever();

    builder.Property(e => e.CreatedAt)
        .IsRequired();

    builder.Property(e => e.UpdatedAt);

    builder.Property(e => e.Version)
      .IsConcurrencyToken()
      .HasDefaultValue(0L);

    builder.Property(e => e.IsDeleted)
        .HasDefaultValue(false);

    builder.Property(e => e.CreatedBy)
        .IsRequired();

    builder.Property(e => e.UpdatedBy);

    builder.HasIndex(e => e.Id);

    builder.HasQueryFilter(nameof(Entity.IsDeleted), e => !e.IsDeleted);
  }
}
