using System.Text.Json.Serialization;

namespace CRISP.Core.Events;

/// <summary>
/// Represents a domain event in the CRISP architecture.
/// Domain events are used to communicate changes or notable occurrences within the domain model.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Gets the unique identifier for this event instance.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Gets the timestamp when the event occurred.
    /// </summary>
    DateTime OccurredOn { get; }

    /// <summary>
    /// Gets the type name of the event for serialization/deserialization.
    /// </summary>
    [JsonIgnore]
    string EventTypeName { get; }
}