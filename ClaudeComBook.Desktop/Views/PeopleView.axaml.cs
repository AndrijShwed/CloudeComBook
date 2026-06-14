using Avalonia.Controls;
using ClaudeComBook.Desktop.Services;

namespace ClaudeComBook.Desktop.Views;

public partial class PeopleView : Window
{
    private readonly Window _previousWindow;

    public PeopleView(Window previousWindow)
    {
        InitializeComponent();
        _previousWindow = previousWindow;
    }

    private void OnOpenSearchWindowClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var window = new PeopleSearchView(this);
        window.Show();
        this.Hide();
    }

    private void OnAddClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // пізніше
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        _previousWindow.Show();
        base.OnClosing(e);
    }
}