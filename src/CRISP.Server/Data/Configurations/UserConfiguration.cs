using CRISP.Core.Identity;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CRISP.Server.Data.Entities;

namespace CRISP.Server.Data.Configurations;

/// <summary>
/// Configures EF Core mapping for <see cref="UserEntity"/>.
/// </summary>
public sealed class UserConfiguration : BaseConfiguration<UserEntity>
{
    /// <summary>
    /// Applies table, property, index, and owned settings configuration for users.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public override void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        base.Configure(builder);

        builder.ToTable(nameof(User).Pluralize());

        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Email)
            .IsRequired();

        builder.Property(u => u.Role)
            .IsRequired();

        builder.Property(u => u.IsActive)
            .HasDefaultValue(true);

        builder.OwnsOne(u => u.Settings, sBuilder =>
        {
            sBuilder.ToTable(nameof(UserSettings));

            sBuilder.Property(s => s.AllowNotifications)
                .HasDefaultValue(true);
        });

        builder.HasIndex(u => u.Username)
            .IsUnique();

        builder.HasIndex(u => u.Email)
            .IsUnique();
    }
}
