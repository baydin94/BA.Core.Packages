using Core.Persistence.Sql.Repositories;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.Persistence.Sql.BaseEntityConfiguration;

public class BaseEntityConfiguration<TEntity, TEntityId> : IEntityTypeConfiguration<TEntity>
    where TEntity : BaseEntity<TEntityId>
{
    public void Configure(EntityTypeBuilder<TEntity> builder)
    {
        string tableName = typeof(TEntity).Name.Pluralize();
        builder.ToTable(tableName);
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("Id").IsRequired();
        builder.Property(e => e.CreatedDate).HasColumnName("CreatedDate").IsRequired();
        builder.Property(e => e.UpdatedDate).HasColumnName("UpdatedDate");
        builder.Property(e => e.DeletedDate).HasColumnName("DeletedDate");

        builder.HasQueryFilter(e => !e.DeletedDate.HasValue); //global filter
    }
}
