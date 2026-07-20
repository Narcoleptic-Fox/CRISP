using CRISP.Server.Common;

namespace CRISP.Server.Events;

/// <summary>
/// Raised after a user's username has been changed and persisted. Asset rows
/// snapshot the owner username, so they must be re-synced to the new value.
/// </summary>
/// <param name="OldUsername">The username before the change.</param>
/// <param name="NewUsername">The username after the change.</param>
public sealed record UsernameChangedEvent(string OldUsername, string NewUsername) : BaseEvent;
