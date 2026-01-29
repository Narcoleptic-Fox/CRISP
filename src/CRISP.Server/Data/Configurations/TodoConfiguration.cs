using CRISP.Server.Data.Entities;
using CRISP.ServiceDefaults.Data.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRISP.Server.Data.Configurations;

public sealed class TodoConfiguration : BaseAuditableConfiguration<TodoEntity>
{
    public override void Configure(EntityTypeBuilder<TodoEntity> builder)
    {
        base.Configure(builder);

        builder.ToTable("Todos");

        builder.Property(t => t.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasMaxLength(2000);

        builder.HasIndex(t => t.IsCompleted);
        builder.HasIndex(t => t.DueDate);
    }
}
