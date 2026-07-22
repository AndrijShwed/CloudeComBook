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
        if (!AppSession.IsUser)
        {
            var err = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Доступ заборонено", "У вас немає прав для додавання!");
            err.ShowAsync();
            return;
        }
        var window = new PersonEditView(this);
        window.Show();
        this.Hide();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        _previousWindow.Show();
        base.OnClosing(e);
    }

    private bool _manualClose = false;
    private void OnHomeClick(object sender, Avalonia.Input.TappedEventArgs e)
    {
        _manualClose = true;
        if (_previousWindow is PeopleView peopleView)
        {
            peopleView._previousWindow.Show();
            peopleView.Hide();
        }
        else
        {
            _previousWindow.Show();
        }
        Close();
    }
}