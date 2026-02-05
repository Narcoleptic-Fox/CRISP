using CRISP.ServiceDefaults.Data.Entities;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text;

namespace CRISP.ServiceDefaults.Data.Interceptors
{
    public sealed class AuditLogInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _currentUser;
        public AuditLogInterceptor(IHttpContextAccessor httpContextAccessors)
        {
            _httpContextAccessor = httpContextAccessors;
            _currentUser = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            AddSystemMessage(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            AddSystemMessage(eventData.Context);
            return base.SavingChangesAsync(eventData, result);
        }

        private void AddSystemMessage(DbContext? context)
        {
            if (context is null)
                return;

            var messagesToAdd = new List<SystemMessageEntity>();
            foreach (EntityEntry<BaseAuditableEntity> entry in context.ChangeTracker.Entries<BaseAuditableEntity>())
            {
                string message = "";
                DateTime changedTime = DateTime.UtcNow;
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedOn = changedTime;
                        message = $"{_currentUser} Added {entry.Entity.GetType().Name.Titleize()}";
                        break;
                    case EntityState.Modified:
                    {
                        entry.Entity.UpdatedOn = changedTime;
                        if (CheckDeleted(entry, out string? deleteMessage))
                        {
                            message = deleteMessage!;
                            break;
                        }
                        else if (CheckArchived(entry, out string? archiveMessage))
                        {
                            message = archiveMessage!;
                            break;
                        }
                        else
                        {
                            message = string.Join(Environment.NewLine,
                                $"{_currentUser} Modified {entry.Entity.GetType().Name.Titleize()} with",
                                GetChangeMessage(entry));
                        }
                    }
                    break;
                    case EntityState.Deleted:
                        entry.Entity.UpdatedOn = changedTime;
                        message = $"{_currentUser} Deleted {entry.Entity.GetType().Name.Titleize()}";
                        break;
                    default:
                        continue;
                }

                var systemMessage = new SystemMessageEntity
                {
                    Content = message,
                    EntityType = entry.Entity.GetType().Name,
                    EntityId = entry.Entity.Id,
                    CreatedOn = changedTime,
                };
                messagesToAdd.Add(systemMessage);
            }
            if (messagesToAdd.Count > 0)
            {
                context.Set<SystemMessageEntity>().AddRange(messagesToAdd);
            }
        }

        private bool CheckDeleted(EntityEntry<BaseAuditableEntity> entry, out string? message)
        {
            message = null;
            object? oldValue = entry.OriginalValues[nameof(BaseAuditableEntity.IsDeleted)];
            object? newValue = entry.CurrentValues[nameof(BaseAuditableEntity.IsDeleted)];
            if (CheckIfRestored(entry, oldValue, newValue, out string? isRestored))
                message = isRestored;
            else if (entry.Entity.IsDeleted)
                message = $"{_currentUser} Deleted {entry.Entity.GetType().Name.Titleize()}";

            return message != null;
        }

        private bool CheckArchived(EntityEntry<BaseAuditableEntity> entry, out string? message)
        {
            message = null;
            object? oldValue = entry.OriginalValues[nameof(BaseAuditableEntity.IsArchived)];
            object? newValue = entry.CurrentValues[nameof(BaseAuditableEntity.IsArchived)];
            if (CheckIfRestored(entry, oldValue, newValue, out string? isRestored))
                message = isRestored;
            else if (entry.Entity.IsArchived)
                message = $"{_currentUser} Archived {entry.Entity.GetType().Name.Titleize()}";

            return message != null;
        }

        private bool CheckIfRestored(EntityEntry<BaseAuditableEntity> entry, object? oldValue, object? newValue, out string? message)
        {
            message = null;
            if (oldValue?.Equals(newValue) == false && newValue is bool newBool && !newBool)
                message = $"{_currentUser} Restored {entry.Entity.GetType().Name.Titleize()}";

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
                    string oldValue = property.OriginalValue?.ToString() ?? "null";
                    string newValue = property.CurrentValue?.ToString() ?? "null";
                    changeBuilder.AppendLine($"{propertyName}: '{oldValue}' → '{newValue}'");
                }
            }
            return changeBuilder.ToString().TrimEnd(Environment.NewLine.ToCharArray());
        }
    }
}
