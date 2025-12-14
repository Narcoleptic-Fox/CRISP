namespace CRISP.Core.Identity;

public sealed record RoleCreated(Guid Id) : BaseEvent;
public sealed record RoleUpdated(Guid Id) : BaseEvent;
public sealed record RoleDeleted(Role Role) : BaseEvent;
