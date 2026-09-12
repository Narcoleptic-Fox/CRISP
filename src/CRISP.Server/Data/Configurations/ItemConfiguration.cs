using CRISP.Core.Items;
using CRISP.Server.Data.Entities;
using CRISP.Server.Data.ValueObjects;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRISP.Server.Data.Configurations;

/// <summary>
/// Configures EF Core mapping for <see cref="ItemEntity"/>.
/// </summary>
public class ItemConfiguration : BaseAuditableConfiguration<ItemEntity>
{
    /// <summary>
    /// Applies table, property, index, and owned file metadata configuration for items.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public override void Configure(EntityTypeBuilder<ItemEntity> builder)
    {
        base.Configure(builder);

        builder.ToTable(nameof(Item).Pluralize());

        builder.Property(i => i.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.Description)
            .HasMaxLength(1000);

        builder.Property(i => i.Owner)
            .HasMaxLength(100);

        builder.Property(i => i.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(i => i.Name)
            .IsUnique();

        builder.HasIndex(i => i.Owner);

        builder.OwnsMany(i => i.Files, fileBuilder =>
        {
            fileBuilder.ToTable("ItemFiles");
            fileBuilder.WithOwner().HasForeignKey("ItemId");
            fileBuilder.HasKey(f => f.Id);
            fileBuilder.Property(f => f.Id).ValueGeneratedNever();

            fileBuilder.Property(f => f.OriginalFileName).IsRequired().HasMaxLength(255);
            fileBuilder.Property(f => f.ObjectKey)
                .IsRequired()
                .HasMaxLength(1024)
                .HasColumnType("varchar(1024)")
                .HasCharSet("latin1");
            fileBuilder.Property(f => f.ContentType).IsRequired().HasMaxLength(255);
            fileBuilder.Property(f => f.UploadedBy).IsRequired().HasMaxLength(100);

            fileBuilder.HasIndex("ItemId", nameof(StoredFileObject.ObjectKey)).IsUnique();
        });
    }
}
