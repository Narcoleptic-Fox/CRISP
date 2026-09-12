using CRISP.Server.Common;
using CRISP.Server.Events;

namespace CRISP.Server.Features.Items.Events;

/// <summary>
/// Example event handler that logs item renames.
/// </summary>
/// <remarks>
/// Demonstrates the in-process event bus end to end: the update service publishes an
/// <see cref="ItemRenamedEvent"/>, the background <c>EventWorker</c> dispatches it, and this
/// handler reacts. Registered as <c>IEventHandler&lt;ItemRenamedEvent&gt;</c> in
/// <see cref="ItemFeature"/>. Add or replace handlers to react to domain events without
/// coupling the publisher to its consumers.
/// </remarks>
internal sealed class ItemRenamedLogger(ILogger<ItemRenamedLogger> logger)
    : IEventHandler<ItemRenamedEvent>
{
    /// <summary>
    /// Logs the item rename.
    /// </summary>
    /// <param name="event">The item rename event.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>A completed task-like value.</returns>
    public ValueTask HandleAsync(ItemRenamedEvent @event, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Item {ItemId} renamed from '{OldName}' to '{NewName}'.",
            @event.ItemId,
            @event.OldName,
            @event.NewName);

        return ValueTask.CompletedTask;
    }
}
