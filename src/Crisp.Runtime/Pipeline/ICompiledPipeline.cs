namespace Crisp.Pipeline
{
    /// <summary>
    /// Base interface for all compiled pipelines, providing unified storage.
    /// </summary>
    internal interface ICompiledPipeline
    {
        Type RequestType { get; }
        Type ResponseType { get; }
        Type HandlerType { get; }
        string HandlerName { get; }
        string RequestName { get; }
        bool IsCommand { get; }
    }

    /// <summary>
    /// Interface for compiled pipelines that return a response.
    /// </summary>
    /// <typeparam name="TResponse">The response type.</typeparam>
    internal interface ICompiledPipeline<TResponse> : ICompiledPipeline
    {
        Task<TResponse> ExecuteAsync(object request, IServiceProvider serviceProvider, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Interface for compiled pipelines that don't return a response (void).
    /// </summary>
    internal interface ICompiledVoidPipeline : ICompiledPipeline
    {
        Task ExecuteAsync(object request, IServiceProvider serviceProvider, CancellationToken cancellationToken);
    }
}
