using Avalonia.Controls;
using ClaudeComBook.Desktop.Services;

namespace ClaudeComBook.Desktop.Views;

public partial class VillagesView : Window
{
    private readonly ApiService _api = new();

    public VillagesView()
    {
        InitializeComponent();
        LoadData();
    }

    private async void LoadData()
    {
        var villages = await _api.GetVillagesAsync();
        VillagesList.ItemsSource = villages;
    }
}