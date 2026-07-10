using Avalonia.Controls;

namespace ClaudeComBook.Desktop.Views;

public partial class EnterprisesView : Window
{
    public readonly Window _previousWindow;
    private bool _manualClose = false;

    public EnterprisesView(Window previousWindow)
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
        var window = new AddEnterpriseView(this);
        window.Show();
        this.Hide();
    }

    private void OnSearchClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var window = new EnterpriseSearchView(this);
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