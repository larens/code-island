using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using CodeIsland.Windows.ViewModels;

namespace CodeIsland.Windows.Views;

public sealed partial class SessionDetailPage : Page
{
    private SessionDetailViewModel ViewModel => (SessionDetailViewModel)DataContext;

    public SessionDetailPage()
    {
        this.InitializeComponent();
        DataContext = App.GetService<SessionDetailViewModel>();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is string sessionId)
        {
            ViewModel.LoadSession(sessionId);
        }
    }

    private void OnBackClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }
}
