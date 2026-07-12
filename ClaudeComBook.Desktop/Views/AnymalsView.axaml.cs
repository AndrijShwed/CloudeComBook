using Avalonia.Controls;

namespace ClaudeComBook.Desktop.Views;

public partial class AnymalsView : Window
{
    public readonly Window _previousWindow;
    private bool _manualClose = false;

    public AnymalsView(Window previousWindow)
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

    private void OnAddClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var window = new AddAnimalView(this);
        window.Show();
        this.Hide();
    }

    private void OnSearchClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var window = new AnimalSearchView(this);
        window.Show();
        this.Hide();
    }
    private void OnStatisticsClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
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