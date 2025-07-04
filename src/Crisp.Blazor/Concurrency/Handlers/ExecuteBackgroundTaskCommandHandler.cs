using Blazor.Concurrency;
using Blazor.Concurrency.Background;
using Blazor.Concurrency.Workers;
using Crisp.Blazor.Concurrency.Commands;
using Crisp.Commands;

namespace Crisp.Blazor.Concurrency.Handlers;

/// <summary>
/// Handler for executing background tasks using web workers.
/// </summary>
public sealed class ExecuteBackgroundTaskCommandHandler<T> : ICommandHandler<ExecuteBackgroundTaskCommand<T>, BackgroundJobResult<T>>
{
    private readonly IWorkerExecutor _workerExecutor;

    public ExecuteBackgroundTaskCommandHandler(IWorkerExecutor workerExecutor) => _workerExecutor = workerExecutor;

    public async Task<BackgroundJobResult<T>> Handle(ExecuteBackgroundTaskCommand<T> request, CancellationToken cancellationToken)
    {
        WorkerOperation operation = WorkerOperation.Background("executeTask", new { request.Task.TaskType, request.Task.Payload });

        Progress<int> progress = request.OnProgress != null
            ? new Progress<int>(p => request.OnProgress(new ParallelProgress(p, 2, [])))
            : null;

        return await _workerExecutor.ExecuteAsync<BackgroundJobResult<T>>(operation, progress, cancellationToken);
    }
}
