using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using CodeIsland.Windows.Models;

namespace CodeIsland.Windows.Views.Controls;

public sealed partial class SessionCard : UserControl
{
    public static readonly DependencyProperty SessionProperty =
        DependencyProperty.Register(nameof(Session), typeof(SessionState), typeof(SessionCard),
            new PropertyMetadata(null, OnSessionChanged));

    public SessionState? Session
    {
        get => (SessionState?)GetValue(SessionProperty);
        set => SetValue(SessionProperty, value);
    }

    public SessionCard()
    {
        this.InitializeComponent();
    }

    private static void OnSessionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SessionCard card && e.NewValue is SessionState session)
        {
            card.ProjectNameText.Text = session.ProjectName;
            card.ProviderText.Text = session.Provider;

            if (!string.IsNullOrEmpty(session.LatestHookMessage))
            {
                card.MessageText.Text = session.LatestHookMessage;
                card.MessageText.Visibility = Visibility.Visible;
            }

            card.AttentionBadge.Visibility = session.NeedsAttention ? Visibility.Visible : Visibility.Collapsed;

            card.StatusDot.Background = session.Phase.Kind switch
            {
                SessionPhase.PhaseKind.Idle => new SolidColorBrush(Colors.Gray),
                SessionPhase.PhaseKind.Processing => new SolidColorBrush(Colors.Green),
                SessionPhase.PhaseKind.WaitingForInput => new SolidColorBrush(Colors.Blue),
                SessionPhase.PhaseKind.WaitingForApproval => new SolidColorBrush(Colors.Orange),
                SessionPhase.PhaseKind.Compacting => new SolidColorBrush(Colors.Yellow),
                SessionPhase.PhaseKind.Ended => new SolidColorBrush(Colors.Gray),
                _ => new SolidColorBrush(Colors.Gray)
            };
        }
    }
}
