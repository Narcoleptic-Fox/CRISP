namespace CRISP.Events
{
    public interface IEventHandler<TEvent> where TEvent : IEvent
    {
        ValueTask Handle(TEvent @event);
    }
}
