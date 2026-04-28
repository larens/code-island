using Microsoft.UI.Xaml;
using H.NotifyIcon;

namespace CodeIsland.Windows.Helpers;

/// <summary>
/// System tray icon management.
/// Uses H.NotifyIcon.WinUi for WinUI 3 tray integration.
/// </summary>
public sealed class TrayIconHelper : IDisposable
{
    private TaskbarIcon? _trayIcon;
    private Window? _mainWindow;

    public void Initialize()
    {
        _trayIcon = new TaskbarIcon
        {
            ToolTipText = "Code Island - AI Session Monitor",
            LeftMouseDownCommand = new DelegateCommand(OnTrayIconClicked),
            DoubleClickCommand = new DelegateCommand(OnTrayIconDoubleClicked)
        };

        // Build context menu
        var menu = new ContextMenu();

        var openItem = new MenuItem { Header = "Open" };
        openItem.Click += (_, _) => ShowMainWindow();
        menu.Items.Add(openItem);

        menu.Items.Add(new Separator());

        var exitItem = new MenuItem { Header = "Exit" };
        exitItem.Click += (_, _) => ExitApplication();
        menu.Items.Add(exitItem);

        _trayIcon.ContextMenu = menu;
    }

    public void SetMainWindow(Window window)
    {
        _mainWindow = window;
    }

    public void ShowNotification(string title, string message, BalloonIconKind kind = BalloonIconKind.Info)
    {
        _trayIcon?.ShowNotification(title, message, kind);
    }

    private void OnTrayIconClicked()
    {
        ShowMainWindow();
    }

    private void OnTrayIconDoubleClicked()
    {
        ShowMainWindow();
    }

    private void ShowMainWindow()
    {
        _mainWindow?.Activate();
    }

    private void ExitApplication()
    {
        Application.Current.Exit();
    }

    public void Dispose()
    {
        _trayIcon?.Dispose();
    }
}
