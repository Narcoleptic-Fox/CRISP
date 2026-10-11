using CRISP.Core.Common;
using CRISP.Server.Common;

namespace CRISP.Server.Tests.TestSupport;

/// <summary>
/// Captures published events in memory so tests can assert what (if anything) was raised.
/// </summary>
internal sealed class RecordingEventPublisher : IEventPublisher
{
    private readonly List<BaseEvent> _published = [];

    public IReadOnlyList<BaseEvent> Published => _published;

    public ValueTask Publish<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : BaseEvent
    {
        _published.Add(@event);
        return ValueTask.CompletedTask;
    }
}

internal sealed class FixedCreateService<TCreate>(Guid id) : ICreateService<TCreate>
    where TCreate : CreateCommand
{
    public ValueTask<Guid> Send(TCreate command, CancellationToken cancellationToken = default) =>
        ValueTask.FromResult(id);
}

internal sealed class RecordingModifyService<TModify> : IModifyService<TModify>
    where TModify : ModifyCommand
{
    public TModify? LastCommand { get; private set; }

    public ValueTask Send(TModify command, CancellationToken cancellationToken = default)
    {
        LastCommand = command;
        return ValueTask.CompletedTask;
    }
}

internal sealed class FixedQueryService<TQuery, TResponse>(TResponse response) : IQueryService<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
    where TResponse : class
{
    public ValueTask<TResponse> Send(TQuery query, CancellationToken cancellationToken = default) =>
        ValueTask.FromResult(response);
}
