namespace CRISP.Core.Audit;

[ExportTsInterface]
public sealed record SystemMessage : BaseModel
{
    /// <summary>
    /// Gets or sets the audit message content.
    /// </summary>
    public IEnumerable<string> Content { get; set; } = default!;
    /// <summary>
    /// Gets or sets the timestamp when the message was created.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }
}
