namespace CRISP.Core.Identity;

public sealed record UserCreated(Guid Id) : BaseEvent;
public sealed record UserUpdated(Guid Id) : BaseEvent;
public sealed record UserDeleted(User User) : BaseEvent;
