using Avalonia.Controls;
using ClaudeComBook.Desktop.Services;

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
        if (!AppSession.IsUser)
        {
            var err = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Доступ заборонено", "У вас немає прав для додавання!");
            err.ShowAsync();
            return;
        }
        var window = new AddAnymalView(this);
        window.Show();
        this.Hide();
    }

    private void OnSearchClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var window = new AnymalSearchView(this);
        window.Show();
        this.Hide();
    }
    private void OnStatisticsClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var window = new AnymalStatisticsView(this);
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