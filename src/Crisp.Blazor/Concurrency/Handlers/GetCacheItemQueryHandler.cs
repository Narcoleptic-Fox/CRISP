using Blazor.Concurrency;
using Blazor.Concurrency.Cache;
using Blazor.Concurrency.Workers;
using Crisp.Concurrency.Queries;
using Crisp.Queries;

namespace Crisp.Blazor.Concurrency.Handlers;

/// <summary>
/// Handler for getting cached items using web workers.
/// </summary>
public sealed class GetCacheItemQueryHandler : IQueryHandler<GetCacheItemQuery, CacheGetResult>
{
    private readonly IWorkerExecutor _workerExecutor;

    public GetCacheItemQueryHandler(IWorkerExecutor workerExecutor) => _workerExecutor = workerExecutor;

    public async Task<CacheGetResult> Handle(GetCacheItemQuery request, CancellationToken cancellationToken)
    {
        WorkerOperation operation = WorkerOperation.Cache("get", new { request.Key });

        return await _workerExecutor.ExecuteAsync<CacheGetResult>(operation, cancellationToken);
    }
}
