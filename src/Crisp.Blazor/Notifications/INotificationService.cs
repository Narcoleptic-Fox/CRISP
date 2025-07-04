namespace Crisp.Notifications;

/// <summary>
/// Service for displaying notifications in Blazor applications.
/// Works with any UI library (MudBlazor, Vibe.UI, etc.).
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Shows a success notification.
    /// </summary>
    void ShowSuccess(string message, string? title = null);

    /// <summary>
    /// Shows an error notification.
    /// </summary>
    void ShowError(string message, string? title = null);

    /// <summary>
    /// Shows an info notification.
    /// </summary>
    void ShowInfo(string message, string? title = null);

    /// <summary>
    /// Shows a warning notification.
    /// </summary>
    void ShowWarning(string message, string? title = null);

    /// <summary>
    /// Event raised when a notification should be displayed.
    /// UI libraries can subscribe to this to show actual notifications.
    /// </summary>
    event EventHandler<NotificationEventArgs>? NotificationRequested;
}

/// <summary>
/// Notification event arguments.
/// </summary>
public class NotificationEventArgs : EventArgs
{
    /// <summary>
    /// Gets the type of the notification.
    /// </summary>
    public NotificationType Type { get; }

    /// <summary>
    /// Gets the message of the notification.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the title of the notification, if any.
    /// </summary>
    public string? Title { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationEventArgs"/> class.
    /// </summary>
    /// <param name="type">The type of the notification.</param>
    /// <param name="message">The message of the notification.</param>
    /// <param name="title">The title of the notification, if any.</param>
    public NotificationEventArgs(NotificationType type, string message, string? title = null)
        => (Type, Message, Title) = (type, message, title);
}

/// <summary>
/// Notification types.
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// Represents a success notification.
    /// </summary>
    Success,

    /// <summary>
    /// Represents an error notification.
    /// </summary>
    Error,

    /// <summary>
    /// Represents an informational notification.
    /// </summary>
    Info,

    /// <summary>
    /// Represents a warning notification.
    /// </summary>
    Warning
}