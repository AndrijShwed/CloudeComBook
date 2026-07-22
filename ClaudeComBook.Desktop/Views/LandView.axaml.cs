using Avalonia.Controls;
using ClaudeComBook.Desktop.Services;

namespace ClaudeComBook.Desktop.Views;

public partial class LandView : Window
{
    public readonly Window _previousWindow;
    private bool _manualClose = false;

    public LandView(Window previousWindow)
    {
        InitializeComponent();
        UserLabel.Text = AppSession.CurrentUser?.FullName ?? AppSession.CurrentUser?.Login ?? "";
        _previousWindow = previousWindow;
    }

    private void OnHomeClick(object sender, Avalonia.Input.TappedEventArgs e)
    {
        _manualClose = true;
        _previousWindow.Show();
        Close();
    }

    private void OnAddClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (!AppSession.IsUser)
        {
            var err = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Доступ заборонено", "У вас немає прав для додавання!");
            err.ShowAsync();
            return;
        }
        var window = new AddLandView(this);
        window.Show();
        this.Hide();
    }

    private void OnSearchClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var window = new LandSearchView(this);
        window.Show();
        this.Hide();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (!_manualClose)
            _previousWindow.Show();
        base.OnClosing(e);
    }
}