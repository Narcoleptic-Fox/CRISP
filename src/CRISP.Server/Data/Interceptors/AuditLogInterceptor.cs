using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using CRISP.Server.Data.Entities;
using System.Collections;
using System.Text;
using System.Text.Json;

namespace CRISP.Server.Data.Interceptors
{
    /// <summary>
    /// Captures audit messages and updates audit fields before EF Core saves changes.
    /// </summary>
    public sealed class AuditLogInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        /// <summary>
        /// Initializes a new instance of the <see cref="AuditLogInterceptor"/> class.
        /// </summary>
        /// <param name="httpContextAccessors">The HTTP context accessor used to identify the current user.</param>
        public AuditLogInterceptor(IHttpContextAccessor httpContextAccessors)
        {
            _httpContextAccessor = httpContextAccessors;
        }

        /// <summary>
        /// Adds audit metadata and system messages before synchronous save changes operations.
        /// </summary>
        /// <param name="eventData">The EF Core save changes event data.</param>
        /// <param name="result">The current interception result.</param>
        /// <returns>The interception result returned by the base interceptor.</returns>
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            AddSystemMessage(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        /// <summary>
        /// Adds audit metadata and system messages before asynchronous save changes operations.
        /// </summary>
        /// <param name="eventData">The EF Core save changes event data.</param>
        /// <param name="result">The current interception result.</param>
        /// <param name="cancellationToken">A token that cancels the save operation.</param>
        /// <returns>The interception result returned by the base interceptor.</returns>
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            AddSystemMessage(eventData.Context);
            return base.SavingChangesAsync(eventData, result);
        }

        private void AddSystemMessage(DbContext? context)
        {
            if (context is null)
                return;

            var currentUser = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";

            var messagesToAdd = new List<SystemMessageEntity>();
            foreach (EntityEntry<BaseAuditableEntity> entry in context.ChangeTracker.Entries<BaseAuditableEntity>())
            {
                string message = "";
                DateTime changedTime = DateTime.UtcNow;
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedOn = entry.Entity.UpdatedOn = changedTime;
                        entry.Entity.CreatedBy = entry.Entity.UpdatedBy = currentUser;

                        message = $"{currentUser} Added {entry.Entity.GetType().Name.Replace("Entity", "").Titleize()}";
                        break;
                    case EntityState.Modified:
                        {
                            entry.Entity.UpdatedOn = changedTime;
                            entry.Entity.UpdatedBy = currentUser;
                            if (CheckDeleted(entry, currentUser, out string? deleteMessage))
                            {
                                message = deleteMessage!;
                                break;
                            }
                            else if (CheckArchived(entry, currentUser, out string? archiveMessage))
                            {
                                message = archiveMessage!;
                                break;
                            }
                            else
                            {
                                string changes = GetChangeMessage(entry);
                                // Nothing auditable actually changed — e.g. an inventory stock op only
                                // bumped the concurrency token (skipped by GetChangeMessage). Skip the
                                // empty "Modified X with" noise row; the UpdatedOn/UpdatedBy stamps above
                                // still apply, and the movement ledger records what actually happened.
                                if (string.IsNullOrWhiteSpace(changes))
                                    continue;

                                message = string.Join(Environment.NewLine,
                                    $"{currentUser} Modified {entry.Entity.GetType().Name.Replace("Entity", "").Titleize()} with",
                                    changes);
                            }
                        }
                        break;
                    case EntityState.Deleted:
                        entry.Entity.UpdatedOn = changedTime;
                        entry.Entity.UpdatedBy = currentUser;
                        message = $"{currentUser} Deleted {entry.Entity.GetType().Name.Replace("Entity", "").Titleize()} with information {JsonSerializer.Serialize(entry.Entity)}";
                        break;
                    default:
                        continue;
                }

                var systemMessage = new SystemMessageEntity
                {
                    Content = message,
                    EntityType = entry.Entity.GetType().Name.Replace("Entity", ""),
                    EntityId = entry.Entity.Id,
                    Timestamp = changedTime,
                };
                messagesToAdd.Add(systemMessage);
            }
            if (messagesToAdd.Count > 0)
            {
                context.Set<SystemMessageEntity>().AddRange(messagesToAdd);
            }
        }

        private bool CheckDeleted(EntityEntry<BaseAuditableEntity> entry, string currentUser, out string? message)
        {
            message = null;
            object? oldValue = entry.OriginalValues[nameof(BaseAuditableEntity.IsDeleted)];
            object? newValue = entry.CurrentValues[nameof(BaseAuditableEntity.IsDeleted)];
            if (CheckIfRestored(entry, oldValue, newValue, currentUser, out string? isRestored))
                message = isRestored;
            else if (entry.Entity.IsDeleted)
                message = $"{currentUser} Deleted {entry.Entity.GetType().Name.Replace("Entity", "").Titleize()}";

            return message != null;
        }

        private bool CheckArchived(EntityEntry<BaseAuditableEntity> entry, string currentUser, out string? message)
        {
            message = null;
            object? oldValue = entry.OriginalValues[nameof(BaseAuditableEntity.IsArchived)];
            object? newValue = entry.CurrentValues[nameof(BaseAuditableEntity.IsArchived)];
            if (CheckIfRestored(entry, oldValue, newValue, currentUser, out string? isRestored))
                message = isRestored;
            else if (entry.Entity.IsArchived)
                message = $"{currentUser} Archived {entry.Entity.GetType().Name.Replace("Entity", "").Titleize()}";

            return message != null;
        }

        private bool CheckIfRestored(EntityEntry<BaseAuditableEntity> entry, object? oldValue, object? newValue, string currentUser, out string? message)
        {
            message = null;
            if (oldValue?.Equals(newValue) == false && newValue is bool newBool && !newBool)
                message = $"{currentUser} Restored {entry.Entity.GetType().Name.Replace("Entity", "").Titleize()}";

            return message != null;
        }

        private string GetChangeMessage(EntityEntry<BaseAuditableEntity> entry)
        {
            var changeBuilder = new StringBuilder();
            foreach (PropertyEntry property in entry.Properties)
            {
                if (property.Metadata.Name == nameof(BaseAuditableEntity.CreatedOn) ||
                    property.Metadata.Name == nameof(BaseAuditableEntity.UpdatedOn) ||
                    property.Metadata.Name == nameof(BaseAuditableEntity.ArchivingReason) ||
                    property.Metadata.Name == nameof(BaseAuditableEntity.IsArchived) ||
                    property.Metadata.Name == nameof(BaseAuditableEntity.IsDeleted))
                    continue;
                // Skip unchanged properties
                if (property.IsModified && !Equals(property.OriginalValue, property.CurrentValue))
                {
                    // Skip concurrency tokens and shadow properties
                    if (property.Metadata.IsConcurrencyToken || property.Metadata.IsShadowProperty())
                        continue;

                    string propertyName = property.Metadata.Name;
                    string oldValue = Describe(property.OriginalValue);
                    string newValue = Describe(property.CurrentValue);
                    changeBuilder.AppendLine($"{propertyName}: '{oldValue}' → '{newValue}'");
                }
            }
            return changeBuilder.ToString().TrimEnd(Environment.NewLine.ToCharArray());
        }

        /// <summary>
        /// Renders a property value for the activity log.
        /// </summary>
        /// <remarks>
        /// Collections need their own arm. A property stored through a value converter is still the
        /// CLR collection here — a <c>List&lt;string&gt;</c> behind a JSON converter, say — and
        /// <c>List&lt;T&gt;.ToString()</c> is the type name, so every edit used to read
        /// "System.Collections.Generic.List`1[System.String]" instead of the values. Byte arrays are
        /// counted rather than listed: they are the one collection whose contents are noise.
        /// </remarks>
        internal static string Describe(object? value) => value switch
        {
            null => "null",
            string text => text,
            byte[] bytes => $"{bytes.Length} bytes",
            IEnumerable items => string.Join(", ", items.Cast<object?>().Select(item => item?.ToString() ?? "null")),
            _ => value.ToString() ?? "null",
        };
    }
}
