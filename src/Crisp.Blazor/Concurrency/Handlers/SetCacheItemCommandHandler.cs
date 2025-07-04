using Blazor.Concurrency;
using Blazor.Concurrency.Cache;
using Blazor.Concurrency.Workers;
using Crisp.Blazor.Concurrency.Commands;
using Crisp.Commands;

namespace Crisp.Blazor.Concurrency.Handlers;

/// <summary>
/// Handler for setting cached items using web workers.
/// </summary>
public sealed class SetCacheItemCommandHandler : ICommandHandler<SetCacheItemCommand, CacheSetResult>
{
    private readonly IWorkerExecutor _workerExecutor;

    public SetCacheItemCommandHandler(IWorkerExecutor workerExecutor) => _workerExecutor = workerExecutor;

    public async Task<CacheSetResult> Handle(SetCacheItemCommand request, CancellationToken cancellationToken)
    {
        WorkerOperation operation = WorkerOperation.Cache("set", request);

        return await _workerExecutor.ExecuteAsync<CacheSetResult>(operation, cancellationToken);
    }
}
