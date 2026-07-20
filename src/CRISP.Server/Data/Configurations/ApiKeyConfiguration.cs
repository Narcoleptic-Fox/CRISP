using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CRISP.Core.Identity;
using CRISP.Server.Data.Entities;

namespace CRISP.Server.Data.Configurations;

/// <summary>
/// Configures EF Core mapping for <see cref="ApiKeyEntity"/>.
/// </summary>
public sealed class ApiKeyConfiguration : BaseConfiguration<ApiKeyEntity>
{
    /// <summary>
    /// Applies table, property, and index configuration for API keys.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public override void Configure(EntityTypeBuilder<ApiKeyEntity> builder)
    {
        base.Configure(builder);

        builder.ToTable(nameof(ApiKey).Pluralize());

        builder.Property(k => k.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(k => k.KeyHash)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(k => k.Prefix)
            .IsRequired()
            .HasMaxLength(10);

        builder.HasIndex(k => k.Prefix);
        builder.HasIndex(k => k.IsActive);

        builder.Property(k => k.Roles)
            .IsRequired();
    }
}
