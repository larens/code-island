using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using CodeIsland.Windows.Models;
using CodeIsland.Windows.ViewModels;

namespace CodeIsland.Windows.Views;

public sealed partial class SessionListPage : Page
{
    private SessionListViewModel ViewModel => (SessionListViewModel)DataContext;

    public SessionListPage()
    {
        this.InitializeComponent();
        DataContext = App.GetService<SessionListViewModel>();
    }

    private void OnSessionItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is SessionState session)
        {
            Frame.Navigate(typeof(SessionDetailPage), session.Id);
        }
    }
}
