using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CodeIsland.Windows.Models;
using CodeIsland.Windows.Services;

namespace CodeIsland.Windows.ViewModels;

/// <summary>
/// ViewModel for the session detail/approval page.
/// </summary>
public sealed partial class SessionDetailViewModel : ObservableObject
{
    private readonly SessionStore _sessionStore;

    [ObservableProperty]
    private SessionState? _session;

    [ObservableProperty]
    private string _toolName = string.Empty;

    [ObservableProperty]
    private string? _toolInput;

    [ObservableProperty]
    private bool _hasPendingPermission;

    [ObservableProperty]
    private bool _hasMessage;

    public SessionDetailViewModel(SessionStore sessionStore)
    {
        _sessionStore = sessionStore;
    }

    public void LoadSession(string sessionId)
    {
        Session = _sessionStore.GetSession(sessionId);
        HasMessage = !string.IsNullOrEmpty(Session?.LatestHookMessage);
    }

    public void SetPendingPermission(string toolName, string? toolInput)
    {
        ToolName = toolName;
        ToolInput = toolInput;
        HasPendingPermission = true;
    }

    [RelayCommand]
    private void ApproveTool()
    {
        if (Session == null) return;

        var pending = Session.ToolTracker.Tools
            .FirstOrDefault(t => t.Value.Status == ToolStatus.Running);

        if (pending.Key != null)
        {
            _sessionStore.Process(new SessionEvent.PermissionApproved(Session.Id, pending.Key));
            HasPendingPermission = false;
        }
    }

    [RelayCommand]
    private void DenyTool()
    {
        if (Session == null) return;

        var pending = Session.ToolTracker.Tools
            .FirstOrDefault(t => t.Value.Status == ToolStatus.Running);

        if (pending.Key != null)
        {
            _sessionStore.Process(new SessionEvent.PermissionDenied(Session.Id, pending.Key, "Denied by user"));
            HasPendingPermission = false;
        }
    }

    [RelayCommand]
    private void AlwaysAllow()
    {
        if (Session == null) return;

        Session.AutoApprovePermissions = true;
        ApproveTool();
    }
}
