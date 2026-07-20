using CRISP.Core.Common;
using CRISP.Core.Identity;

namespace CRISP.Server.Data.Entities;

/// <summary>
/// Represents the persisted form of an application user.
/// </summary>
public sealed class UserEntity : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique username.
    /// </summary>
    public string Username { get; set; } = default!;

    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    public string Email { get; set; } = default!;

    /// <summary>
    /// Gets or sets the user's authorization role.
    /// </summary>
    public Roles Role { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the user's application preferences.
    /// </summary>
    public UserSettings Settings { get; set; } = new UserSettings();

    /// <summary>
    /// Converts the persisted entity into the Core user contract.
    /// </summary>
    /// <returns><see cref="User"/>.</returns>
    public User ToUser() => new()
    {
        Id = Id,
        Username = Username,
        Email = Email,
        Role = Role,
        IsActive = IsActive,
        Settings = Settings ?? new UserSettings()
    };

    /// <summary>
    /// Converts the persisted entity into an autocomplete model for lightweight user references.
    /// </summary>
    /// <returns><see cref="AutoComplete"/></returns>
    public AutoComplete ToAutoComplete() => new()
    {
        Id = Id,
        Name = Username
    };
}

/// <summary>
/// Provides projection helpers for user queries.
/// </summary>
public static class UserEntityExtensions
{
    extension(IQueryable<UserEntity> users)
    {
        /// <summary>
        /// Projects user entities to Core user models in a queryable expression.
        /// </summary>
        /// <returns>A queryable projection to <see cref="User"/>.</returns>
        public IQueryable<User> ProjectToUser() =>
             users.Select(user => new User
             {
                 Id = user.Id,
                 Username = user.Username,
                 Email = user.Email,
                 Role = user.Role,
                 IsActive = user.IsActive,
                 Settings = user.Settings ?? new UserSettings()
             });

        /// <summary>
        /// Projects user entities to autocomplete models in a queryable expression.
        /// </summary>
        /// <returns>A queryable projection to <see cref="AutoComplete"/>.</returns>
        public IQueryable<AutoComplete> ProjectToAutoComplete() =>
            users.Select(user => new AutoComplete
            {
                Id = user.Id,
                Name = user.Username
            });
    }
}
