using Microsoft.UI.Xaml.Controls;
using CodeIsland.Windows.ViewModels;

namespace CodeIsland.Windows.Views;

public sealed partial class SettingsPage : Page
{
    private SettingsViewModel ViewModel => (SettingsViewModel)DataContext;

    public SettingsPage()
    {
        this.InitializeComponent();
        DataContext = App.GetService<SettingsViewModel>();
    }
}
