using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CodeIsland.Windows.Services;
using CodeIsland.Windows.Views;

namespace CodeIsland.Windows.ViewModels;

/// <summary>
/// ViewModel for the main window.
/// </summary>
public sealed partial class MainViewModel : ObservableObject
{
    private readonly SessionStore _sessionStore;
    private readonly SettingsService _settingsService;
    private MainWindow? _mainWindow;

    [ObservableProperty]
    private int _attentionCount;

    [ObservableProperty]
    private string _statusText = "No active sessions";

    public MainViewModel(SessionStore sessionStore, SettingsService settingsService)
    {
        _sessionStore = sessionStore;
        _settingsService = settingsService;

        _sessionStore.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SessionStore.AttentionCount))
            {
                AttentionCount = _sessionStore.AttentionCount;
                StatusText = AttentionCount > 0
                    ? $"{AttentionCount} session(s) need attention"
                    : "No active sessions";
            }
        };
    }

    public void SetMainWindow(MainWindow window)
    {
        _mainWindow = window;
    }

    [RelayCommand]
    private void NavigateToSessionList()
    {
        _mainWindow?.NavigateTo(typeof(SessionListPage));
    }

    [RelayCommand]
    private void NavigateToSettings()
    {
        _mainWindow?.NavigateTo(typeof(SettingsPage));
    }
}
