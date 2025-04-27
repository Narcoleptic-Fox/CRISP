namespace CRISP.Core.Events;

/// <summary>
/// Base implementation of <see cref="IDomainEvent"/> that provides common functionality for all domain events.
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DomainEvent"/> class.
    /// </summary>
    protected DomainEvent()
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
    }

    /// <inheritdoc />
    public Guid EventId { get; }
    
    /// <inheritdoc />
    public DateTime OccurredOn { get; }
    
    /// <inheritdoc />
    public string EventTypeName => GetType().Name;
}