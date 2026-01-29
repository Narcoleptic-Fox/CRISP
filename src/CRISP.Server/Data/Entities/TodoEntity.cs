using CRISP.ServiceDefaults.Data.Entities;

namespace CRISP.Server.Data.Entities;

/// <summary>
/// Todo item database entity with auditing and soft delete support.
/// </summary>
public sealed class TodoEntity : BaseAuditableEntity
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
}
