using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CodeIsland.Windows.Models;
using CodeIsland.Windows.Services;

namespace CodeIsland.Windows.ViewModels;

/// <summary>
/// ViewModel for the session list page.
/// </summary>
public sealed partial class SessionListViewModel : ObservableObject
{
    private readonly SessionStore _sessionStore;

    [ObservableProperty]
    private ObservableCollection<SessionState> _sessions = [];

    [ObservableProperty]
    private bool _isEmpty = true;

    [ObservableProperty]
    private string _emptyMessage = "No active sessions. Start an AI coding tool to begin monitoring.";

    public SessionListViewModel(SessionStore sessionStore)
    {
        _sessionStore = sessionStore;

        _sessionStore.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SessionStore.ActiveSessions))
            {
                Sessions = _sessionStore.ActiveSessions;
                IsEmpty = Sessions.Count == 0;
            }
        };

        _sessionStore.SessionAdded += (_, session) =>
        {
            Sessions = _sessionStore.ActiveSessions;
            IsEmpty = Sessions.Count == 0;
        };

        _sessionStore.SessionRemoved += (_, _) =>
        {
            Sessions = _sessionStore.ActiveSessions;
            IsEmpty = Sessions.Count == 0;
        };
    }

    [RelayCommand]
    private void RefreshSessions()
    {
        Sessions = _sessionStore.ActiveSessions;
        IsEmpty = Sessions.Count == 0;
    }
}
