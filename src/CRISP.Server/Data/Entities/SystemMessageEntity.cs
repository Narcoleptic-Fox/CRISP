using CRISP.Core.Audit;
using CRISP.Server.Data.Entities;

namespace CRISP.Server.Data.Entities
{
    /// <summary>
    /// Represents an audit message describing a system change for an entity.
    /// </summary>
    public class SystemMessageEntity : BaseEntity
    {
        /// <summary>
        /// Gets or sets the identifier of the entity described by the message.
        /// </summary>
        public Guid EntityId { get; set; }

        /// <summary>
        /// Gets or sets the entity type described by the message.
        /// </summary>
        public string EntityType { get; set; } = default!;

        /// <summary>
        /// Gets or sets the audit message content.
        /// </summary>
        public string Content { get; set; } = default!;

        /// <summary>
        /// Gets or sets the timestamp when the message was created.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

        public SystemMessage ToSystemMessage() => new SystemMessage
        {
            Id = Id,
            Content = Content.Split(Environment.NewLine),
            Timestamp = Timestamp
        };
    }
}

public static class SystemMessageEntityExtensions
{
    extension(IQueryable<SystemMessageEntity> messages)
    {
        public IQueryable<SystemMessage> ProjectToSystemMessage() =>
            messages.Select(m => new SystemMessage
            {
                Id = m.Id,
                Content = m.Content.Split(Environment.NewLine),
                Timestamp = m.Timestamp
            });

    }
}