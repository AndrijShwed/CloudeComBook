using Avalonia.Controls;
using ClaudeComBook.Desktop.Services;

namespace ClaudeComBook.Desktop.Views;

public partial class HouseholdsView : Window
{
    public readonly Window _previousWindow;
    private bool _manualClose = false;

    public HouseholdsView(Window previousWindow)
    {
        InitializeComponent();
        _previousWindow = previousWindow;
    }

    private void OnHomeClick(object sender, Avalonia.Input.TappedEventArgs e)
    {
        _manualClose = true;
        _previousWindow.Show();
        Close();
    }

    private void OnAddHouseClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (!AppSession.IsUser)
        {
            var err = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Доступ заборонено", "У вас немає прав для додавання!");
            err.ShowAsync();
            return;
        }
        var window = new AddHouseView(this);
        window.Show();
        this.Hide();
    }

    private void OnSearchHouseClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var window = new HouseSearchView(this);
        window.Show();
        this.Hide();
    }

    private void OnAreaClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var window = new HouseAreaView(this);
        window.Show();
        this.Hide();
    }

    private void OnRoomsClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var window = new HouseRoomsView(this);
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