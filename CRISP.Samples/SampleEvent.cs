using CRISP.Events;

namespace CRISP.Samples
{
    public class SampleEvent : IEvent
    {
        public string Message { get; set; } = string.Empty;
    }
}