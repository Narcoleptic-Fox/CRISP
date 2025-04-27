using CRISP.Core.Interfaces;
using CRISP.Core.Responses;

namespace CRISP.Core.Queries;

/// <summary>
/// Base class for queries that return data of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TResult">The type of data returned by the query.</typeparam>
public abstract class Query<TResult> : IRequest<Response<TResult>>
{
    /// <summary>
    /// Gets or sets a unique identifier for the query.
    /// </summary>
    public Guid QueryId { get; set; } = Guid.NewGuid();
}