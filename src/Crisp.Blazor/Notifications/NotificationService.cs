using Crisp.Notifications;

namespace Crisp.Blazor.Notifications;

/// <summary>
/// Default implementation of the notification service.
/// UI libraries can subscribe to the NotificationRequested event to display notifications.
/// </summary>
public class NotificationService : INotificationService
{
    /// <inheritdoc/>
    public event EventHandler<NotificationEventArgs>? NotificationRequested;

    /// <inheritdoc/>
    public void ShowSuccess(string message, string? title = null) =>
        OnNotificationRequested(new NotificationEventArgs(
            NotificationType.Success,
            message,
            title ?? "Success"));

    /// <inheritdoc/>
    public void ShowError(string message, string? title = null) =>
        OnNotificationRequested(new NotificationEventArgs(
            NotificationType.Error,
            message,
            title ?? "Error"));

    /// <inheritdoc/>
    public void ShowInfo(string message, string? title = null) =>
        OnNotificationRequested(new NotificationEventArgs(
            NotificationType.Info,
            message,
            title ?? "Information"));

    /// <inheritdoc/>
    public void ShowWarning(string message, string? title = null) =>
        OnNotificationRequested(new NotificationEventArgs(
            NotificationType.Warning,
            message,
            title ?? "Warning"));

    /// <summary>
    /// Raises the <see cref="NotificationRequested"/> event with the specified event arguments.
    /// </summary>
    /// <param name="e">The event arguments containing notification details.</param>
    protected virtual void OnNotificationRequested(NotificationEventArgs e) =>
        NotificationRequested?.Invoke(this, e);
}
