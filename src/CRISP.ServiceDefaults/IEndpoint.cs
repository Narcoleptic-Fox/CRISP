using CRISP.Core.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Routing;

namespace CRISP.ServiceDefaults
{
    public interface IEndpoint
    {
        static abstract RouteGroupBuilder MapEndpoint(RouteGroupBuilder app);
    }

    public interface ICommandEndpoint<TCommand> : IEndpoint
        where TCommand : ICommand
    {
        static abstract Task<IResult> Handle(
            [FromBody] TCommand command,
            [FromServices] ICommandService<TCommand> service,
            [FromServices] IEventDispatcher eventDispatcher,
            [FromServices] IOutputCacheStore cache,
            CancellationToken cancellationToken);
    }

    public interface ICommandEndpoint<TCommand, TResult> : IEndpoint
        where TCommand : ICommand<TResult>
    {
        static abstract Task<IResult> Handle(
            [FromBody] TCommand command,
            [FromServices] ICommandService<TCommand, TResult> service,
            [FromServices] IEventDispatcher eventDispatcher,
            [FromServices] IOutputCacheStore cache,
            CancellationToken cancellationToken);
    }

    public interface ICreateEndpoint<TCommand> : IEndpoint
        where TCommand : CreateCommand
    {
        static abstract Task<IResult> Handle(
            [FromBody] TCommand command,
            [FromServices] ICreateService<TCommand> service,
            [FromServices] IEventDispatcher eventDispatcher,
            [FromServices] IOutputCacheStore cache,
            CancellationToken cancellationToken);
    }

    public interface IModifyEndpoint<TCommand> : IEndpoint
        where TCommand : ModifyCommand
    {
        static abstract Task<IResult> Handle(
            [FromBody] TCommand command,
            [FromServices] IModifyService<TCommand> service,
            [FromServices] IEventDispatcher eventDispatcher,
            [FromServices] IOutputCacheStore cache,
            CancellationToken cancellationToken);
    }

    public interface IQueryEndpoint<TQuery, TResult> : IEndpoint
        where TQuery : IQuery<TResult>
        where TResult : BaseModel
    {
        static abstract Task<IResult> Handle(
            [AsParameters] TQuery query,
            [FromServices] IQueryService<TQuery, TResult> service,
            CancellationToken cancellationToken);
    }
    public interface ISingularQueryEndpoint<TResult> : IEndpoint
        where TResult : BaseModel
    {
        static abstract Task<IResult> Handle(
            [AsParameters] SingularQuery<TResult> query,
            [FromServices] IQueryService<SingularQuery<TResult>, TResult> service,
            CancellationToken cancellationToken);
    }

    public interface IPagedQueryEndpoint<TQuery, TResult> : IEndpoint
        where TQuery : PagedQuery<TResult>
        where TResult : BaseModel
    {
        static abstract Task<IResult> Handle(
            [AsParameters] TQuery query,
            [FromServices] IQueryService<TQuery, PagedResponse<TResult>> service,
            CancellationToken cancellationToken);
    }
}
