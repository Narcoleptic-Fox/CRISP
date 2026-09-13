using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CRISP.Server.Data.Entities;
using CRISP.Server.Data.ValueObjects;
using System.Linq.Expressions;

namespace CRISP.Server.Data.Configurations
{
    /// <summary>
    /// Provides shared EF Core mapping for entities with generated identifiers.
    /// </summary>
    /// <typeparam name="T">The entity type being configured.</typeparam>
    public abstract class BaseConfiguration<T> : IEntityTypeConfiguration<T>
        where T : BaseEntity
    {
        /// <summary>
        /// Configures the primary key and generated identifier behavior.
        /// </summary>
        /// <param name="builder">The entity type builder.</param>
        public virtual void Configure(EntityTypeBuilder<T> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .ValueGeneratedOnAdd();
        }

        protected void ConfigureStoredFile(EntityTypeBuilder<T> builder, string tableName, string foreignKey)
        {

        }
        protected void ConfigureStoredFiles(EntityTypeBuilder<T> builder, string tableName, string foreignKey)
        {

        }
    }

    /// <summary>
    /// Provides shared EF Core mapping for auditable entities that support archival and soft delete.
    /// </summary>
    /// <typeparam name="T">The auditable entity type being configured.</typeparam>
    public abstract class BaseAuditableConfiguration<T> : BaseConfiguration<T>
        where T : BaseAuditableEntity
    {
        /// <summary>
        /// Configures audit fields, soft-delete query filtering, and archival defaults.
        /// </summary>
        /// <param name="builder">The entity type builder.</param>
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


    public static class EntityTypeBuilderExtensions
    {
        public static void OwnsFiles<TEntity>(this EntityTypeBuilder<TEntity> builder, string tableName, string foreignKey)
            where TEntity : BaseEntity, IHasStoredFiles =>
            builder.OwnsFiles(e => e.Files, tableName, foreignKey);

        public static void OwnsFiles<TEntity>(this EntityTypeBuilder<TEntity> builder, Expression<Func<TEntity, IEnumerable<StoredFileObject>?>> navigationExpression, string tableName, string foreignKey)
            where TEntity : BaseEntity
        {
            if (string.IsNullOrEmpty(tableName))
                tableName = $"{typeof(TEntity).Name}Files";
            if (string.IsNullOrEmpty(foreignKey))
                foreignKey = $"{typeof(TEntity).Name}Id";

            builder.OwnsMany(navigationExpression, fileBuilder =>
            {
                fileBuilder.ToTable(tableName);
                fileBuilder.WithOwner().HasForeignKey(foreignKey);
                fileBuilder.HasKey(f => f.Id);
                fileBuilder.Property(f => f.Id).ValueGeneratedNever();

                fileBuilder.Property(f => f.OriginalFileName).IsRequired().HasMaxLength(255);
                DatabaseProvider.ObjectKeyColumn(fileBuilder.Property(f => f.ObjectKey).IsRequired());
                fileBuilder.Property(f => f.ContentType).IsRequired().HasMaxLength(255);
                fileBuilder.Property(f => f.UploadedBy).IsRequired().HasMaxLength(100);

                fileBuilder.HasIndex(foreignKey, nameof(StoredFileObject.ObjectKey)).IsUnique();
            });
        }

        public static void OwnsQRCode<TEntity>(this EntityTypeBuilder<TEntity> builder, string tableName, string foreignKey)
            where TEntity : BaseEntity, IHasQrCode =>
            builder.OwnsFile(e => e.QRCode, tableName, foreignKey);

        public static void OwnsFile<TEntity>(this EntityTypeBuilder<TEntity> builder, Expression<Func<TEntity, StoredFileObject?>> navigationExpression, string tableName, string foreignKey)
            where TEntity : BaseEntity
        {
            if (string.IsNullOrEmpty(tableName))
                tableName = $"{typeof(TEntity).Name}File";
            if (string.IsNullOrEmpty(foreignKey))
                foreignKey = $"{typeof(TEntity).Name}Id";

            builder.OwnsOne(navigationExpression, fileBuilder =>
            {
                fileBuilder.ToTable(tableName);
                fileBuilder.WithOwner().HasForeignKey(foreignKey);
                fileBuilder.HasKey(f => f.Id);
                fileBuilder.Property(f => f.Id).ValueGeneratedNever();

                fileBuilder.Property(f => f.OriginalFileName).IsRequired().HasMaxLength(255);
                DatabaseProvider.ObjectKeyColumn(fileBuilder.Property(f => f.ObjectKey).IsRequired());
                fileBuilder.Property(f => f.ContentType).IsRequired().HasMaxLength(255);
                fileBuilder.Property(f => f.UploadedBy).IsRequired().HasMaxLength(100);

                fileBuilder.HasIndex(foreignKey, nameof(StoredFileObject.ObjectKey)).IsUnique();
            });
        }
    }
}
