using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using CodeIsland.Windows.Services;
using CodeIsland.Windows.Views;

namespace CodeIsland.Windows;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();

        Title = "Code Island";
        ExtendsContentIntoTitleBar = true;

        ContentFrame.Navigated += OnNavigated;
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        // Handle navigation if needed
    }

    public void NavigateTo(Type pageType, object? parameter = null)
    {
        ContentFrame.Navigate(pageType, parameter);
    }
}
