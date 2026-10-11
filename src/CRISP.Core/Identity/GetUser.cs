namespace CRISP.Core.Identity;

/// <summary>
/// Represents a query for retrieving a single user by identifier.
/// </summary>
public sealed record GetUser : SingularQuery<User>;
