namespace Crisp.Events;

/// <summary>
/// Marker interface for domain events
/// </summary>
public interface IEvent
{
    DateTime OccurredOn { get; }
}