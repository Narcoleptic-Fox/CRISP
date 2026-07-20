using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CRISP.Server.Data.Entities;

namespace CRISP.Server.Data.Configurations;

/// <summary>
/// Configures EF Core mapping for <see cref="SystemMessageEntity"/>.
/// </summary>
public class SystemMessageConfiguration : BaseConfiguration<SystemMessageEntity>
{
    /// <summary>
    /// Applies table and required property configuration for system messages.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public override void Configure(EntityTypeBuilder<SystemMessageEntity> builder)
    {
        base.Configure(builder);

        builder.ToTable("SystemMessages");

        builder.Property(e => e.EntityId)
            .IsRequired();

        builder.Property(e => e.EntityType)
            .IsRequired();

        builder.Property(e => e.Content)
            .IsRequired();
    }
}
