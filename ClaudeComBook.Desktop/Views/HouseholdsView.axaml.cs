using Avalonia.Controls;

namespace ClaudeComBook.Desktop.Views;

public partial class HouseholdsView : Window
{
    private readonly Window _previousWindow;
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
        // Пізніше
    }

    private void OnSearchHouseClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Пізніше
    }

    private void OnAreaClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Пізніше
    }

    private void OnRoomsClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Пізніше
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (!_manualClose)
            _previousWindow.Show();
        base.OnClosing(e);
    }
}