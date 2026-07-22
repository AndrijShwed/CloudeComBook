using Avalonia.Controls;
using ClaudeComBook.Desktop.Services;

namespace ClaudeComBook.Desktop.Views;

public partial class VillagesView : Window
{
    private readonly Window _previousWindow;
    private readonly ApiService _api = new();

    public VillagesView(Window previousWindow)
    {
        InitializeComponent();
        UserLabel.Text = AppSession.CurrentUser?.FullName ?? AppSession.CurrentUser?.Login ?? "";
        _previousWindow = previousWindow;
        LoadData();
    }

    private async void LoadData()
    {
        var villages = await _api.GetVillagesAsync();
        VillagesList.ItemsSource = villages;
    }
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        _previousWindow.Show();
        base.OnClosing(e);
    }
}