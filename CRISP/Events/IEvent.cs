namespace CRISP.Events
{
    /// <summary>
    /// Represents an event in the system using the CQRS pattern.
    /// </summary>
    /// <remarks>
    /// Events are immutable objects that represent something that has occurred in the system.
    /// They are used for notification purposes and to maintain a history of significant actions.
    /// Implement this interface to create custom event types that can be published through the event service.
    /// </remarks>
    public interface IEvent
    {
    }
}
