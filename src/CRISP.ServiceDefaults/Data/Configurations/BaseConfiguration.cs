using CRISP.ServiceDefaults.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRISP.ServiceDefaults.Data.Configurations
{
    public abstract class BaseConfiguration<T> : IEntityTypeConfiguration<T>
        where T : BaseEntity
    {
        public virtual void Configure(EntityTypeBuilder<T> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .ValueGeneratedOnAdd();
        }
    }

    public abstract class BaseAuditableConfiguration<T> : BaseConfiguration<T>
        where T : BaseAuditableEntity
    {
        public override void Configure(EntityTypeBuilder<T> builder)
        {
            base.Configure(builder);

            builder.Property(e => e.CreatedOn)
                .IsRequired();

            builder.Property(e => e.UpdatedOn)
                .IsRequired();

            builder.Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.Property(e => e.IsArchived)
                .HasDefaultValue(false);
        }
    }
}
