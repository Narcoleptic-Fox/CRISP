using CRISP.Server.Common;

namespace CRISP.Server.Events;

/// <summary>
/// Raised after an item's name has been changed and persisted. Published by the item
/// update service and dispatched to every registered handler by the in-process event bus.
/// </summary>
/// <param name="ItemId">The identifier of the renamed item.</param>
/// <param name="OldName">The item name before the change.</param>
/// <param name="NewName">The item name after the change.</param>
public sealed record ItemRenamedEvent(Guid ItemId, string OldName, string NewName) : BaseEvent;
