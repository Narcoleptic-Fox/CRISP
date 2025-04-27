using CRISP.Core.Interfaces;
using CRISP.Core.Responses;

namespace CRISP.Core.Commands;

/// <summary>
/// Base class for commands that do not return data.
/// </summary>
public abstract class Command : IRequest<Response>
{
    /// <summary>
    /// Gets or sets a unique identifier for the command.
    /// </summary>
    public Guid CommandId { get; set; } = Guid.NewGuid();
}

/// <summary>
/// Base class for commands that return data of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TResult">The type of data returned by the command.</typeparam>
public abstract class Command<TResult> : IRequest<Response<TResult>>
{
    /// <summary>
    /// Gets or sets a unique identifier for the command.
    /// </summary>
    public Guid CommandId { get; set; } = Guid.NewGuid();
}