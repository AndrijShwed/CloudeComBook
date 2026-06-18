using Avalonia.Controls;
using ClaudeComBook.Desktop.Services;

namespace ClaudeComBook.Desktop.Views;

public partial class PeopleView : Window
{
    public readonly Window _previousWindow;

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
        var window = new PersonEditView(this);
        window.Show();
        this.Hide();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        _previousWindow.Show();
        base.OnClosing(e);
    }
}