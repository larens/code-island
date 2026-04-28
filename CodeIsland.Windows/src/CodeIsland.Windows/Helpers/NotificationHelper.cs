using H.NotifyIcon;
using CodeIsland.Windows.Services;

namespace CodeIsland.Windows.Helpers;

/// <summary>
/// Notification helper for displaying session alerts.
/// </summary>
public sealed class NotificationHelper
{
    private readonly SessionStore _sessionStore;
    private readonly TrayIconHelper _trayIcon;

    public NotificationHelper(SessionStore sessionStore, TrayIconHelper trayIcon)
    {
        _sessionStore = sessionStore;
        _trayIcon = trayIcon;

        _sessionStore.PermissionRequested += OnPermissionRequested;
        _sessionStore.SessionAdded += OnSessionAdded;
    }

    private void OnPermissionRequested(object? sender, (string SessionId, string ToolUseId, string ToolName, string? ToolInput) e)
    {
        _trayIcon.ShowNotification(
            "Permission Required",
            $"Tool '{e.ToolName}' needs your approval.",
            BalloonIconKind.Warning);
    }

    private void OnSessionAdded(object? sender, Models.SessionState e)
    {
        _trayIcon.ShowNotification(
            "New Session",
            $"Session started in {e.ProjectName}",
            BalloonIconKind.Info);
    }
}
