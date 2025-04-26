using CRISP.Events;

namespace CRISP
{
    public interface IEventService
    {
        ValueTask Publish(IEvent @event);
    }
}
