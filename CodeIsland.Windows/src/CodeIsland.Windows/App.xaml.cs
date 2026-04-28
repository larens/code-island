using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using CodeIsland.Windows.Services;
using CodeIsland.Windows.Helpers;
using CodeIsland.Windows.ViewModels;

namespace CodeIsland.Windows;

public partial class App : Application
{
    private readonly IHost _host;
    private TrayIconHelper? _trayIcon;

    public static T GetService<T>() where T : class
    {
        var app = (App)Current;
        return app._host.Services.GetRequiredService<T>();
    }

    public App()
    {
        this.InitializeComponent();

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Services
                services.AddSingleton<SettingsService>();
                services.AddSingleton<SessionStore>();
                services.AddSingleton<NamedPipeServer>();
                services.AddSingleton<TcpBridgeServer>();
                services.AddSingleton<HookInstaller>();
                services.AddSingleton<ProcessExecutor>();

                // ViewModels
                services.AddSingleton<MainViewModel>();
                services.AddSingleton<SessionListViewModel>();
                services.AddTransient<SessionDetailViewModel>();
                services.AddSingleton<SettingsViewModel>();
            })
            .Build();
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        await _host.StartAsync();

        var settings = _host.Services.GetRequiredService<SettingsService>();
        settings.Load();

        var pipeServer = _host.Services.GetRequiredService<NamedPipeServer>();
        var tcpServer = _host.Services.GetRequiredService<TcpBridgeServer>();

        await pipeServer.StartAsync();
        await tcpServer.StartAsync();

        _trayIcon = new TrayIconHelper();
        _trayIcon.Initialize();

        MainWindow = new MainWindow();
        MainWindow.Activate();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        _trayIcon?.Dispose();

        var pipeServer = _host.Services.GetService<NamedPipeServer>();
        if (pipeServer != null)
            await pipeServer.StopAsync();

        var tcpServer = _host.Services.GetService<TcpBridgeServer>();
        if (tcpServer != null)
            await tcpServer.StopAsync();

        await _host.StopAsync();
        _host.Dispose();

        base.OnExit(e);
    }
}
