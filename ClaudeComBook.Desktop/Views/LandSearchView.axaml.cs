using Avalonia.Controls;
using Avalonia.Interactivity;
using ClaudeComBook.Desktop.Models;
using ClaudeComBook.Desktop.Services;
using System.Linq;

namespace ClaudeComBook.Desktop.Views;

public partial class LandSearchView : Window
{
    private readonly Window _previousWindow;
    private readonly ApiService _api = new();
    private bool _manualClose = false;

    public LandSearchView(Window previousWindow)
    {
        InitializeComponent();
        _previousWindow = previousWindow;
        LoadComboBoxes();
        VillageBox.SelectionChanged += OnVillageChanged;
        FullNameBox.TextChanged += OnNameTextChanged;
    }

    private async void LoadComboBoxes()
    {
        var villages = await _api.GetVillagesAsync();
        VillageBox.ItemsSource = villages;
        VillageBox.DisplayMemberBinding = new Avalonia.Data.Binding("Name");
    }

    private async void OnVillageChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (VillageBox.SelectedItem is not Village village) return;

        var villageStreets = await _api.GetVillageStreetsAsync();
        var streetIds = villageStreets?
            .Where(vs => vs.VillageId == village.Id && vs.IsActive)
            .Select(vs => vs.StreetId)
            .ToList();

        var allStreets = await _api.GetStreetsAsync();
        var filteredStreets = allStreets?
            .Where(s => streetIds != null && streetIds.Contains(s.Id))
            .ToList();

        StreetBox.ItemsSource = filteredStreets;
        StreetBox.DisplayMemberBinding = new Avalonia.Data.Binding("Name");
        StreetBox.SelectedIndex = -1;
    }

    private void OnNameTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is TextBox box && !string.IsNullOrEmpty(box.Text))
        {
            var text = box.Text;
            var capitalized = char.ToUpper(text[0]) + text.Substring(1);
            if (text != capitalized)
            {
                box.Text = capitalized;
                box.CaretIndex = box.Text.Length;
            }
        }
    }

    public async void OnSearchClick(object? sender, RoutedEventArgs? e)
    {
        var selectedVillage = VillageBox.SelectedItem as Village;
        var selectedStreet = StreetBox.SelectedItem as Street;

        var plots = await _api.SearchPlotsAsync(
            fullName: string.IsNullOrEmpty(FullNameBox.Text) ? null : FullNameBox.Text,
            village: selectedVillage?.Name,
            street: selectedStreet?.Name,
            houseNumb: string.IsNullOrEmpty(HouseNumberBox.Text) ? null : HouseNumberBox.Text,
            fieldNumber: string.IsNullOrEmpty(FieldNumberBox.Text) ? null : FieldNumberBox.Text,
            plotType: string.IsNullOrEmpty(PlotTypeBox.Text) ? null : PlotTypeBox.Text,
            plotNumber: string.IsNullOrEmpty(PlotNumberBox.Text) ? null : PlotNumberBox.Text,
            tenant: string.IsNullOrEmpty(TenantBox.Text) ? null : TenantBox.Text,
            cadastr: string.IsNullOrEmpty(CadastrBox.Text) ? null : CadastrBox.Text);

        LandGrid.ItemsSource = plots;
        CountText.Text = plots?.Count.ToString() ?? "0";
        TotalAreaText.Text = plots?.Sum(p => p.PlotArea ?? 0).ToString("F4") ?? "0";
    }

    private void OnClearClick(object sender, RoutedEventArgs e)
    {
        FullNameBox.Text = "";
        VillageBox.SelectedIndex = -1;
        StreetBox.SelectedIndex = -1;
        HouseNumberBox.Text = "";
        FieldNumberBox.Text = "";
        PlotTypeBox.Text = "";
        PlotNumberBox.Text = "";
        TenantBox.Text = "";
        CadastrBox.Text = "";
    }

    private void OnClearTableClick(object sender, RoutedEventArgs e)
    {
        LandGrid.ItemsSource = null;
        CountText.Text = "0";
        TotalAreaText.Text = "0";
    }

    private async void OnDeleteClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is Plot plot)
        {
            var msg = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Підтвердження",
                    $"Ви дійсно хочете видалити ділянку \"{plot.FullName}\" - {plot.Cadastr}?",
                    MsBox.Avalonia.Enums.ButtonEnum.YesNo);
            var result = await msg.ShowAsync();

            if (result == MsBox.Avalonia.Enums.ButtonResult.Yes)
            {
                await _api.DeletePlotAsync(plot.Id);
                OnSearchClick(null, null);
            }
        }
    }

    private void OnRowDoubleTapped(object sender, Avalonia.Input.TappedEventArgs e)
    {
        if (LandGrid.SelectedItem is Plot plot)
        {
            var window = new LandEditView(plot, this);
            window.Show();
            this.Hide();
        }
    }

    private void OnHomeClick(object sender, Avalonia.Input.TappedEventArgs e)
    {
        _manualClose = true;
        if (_previousWindow is LandView landView)
            landView._previousWindow.Show();
        _previousWindow.Close();
        Close();
    }

    private void OnLandClick(object sender, Avalonia.Input.TappedEventArgs e)
    {
        _previousWindow.Show();
        Close();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (!_manualClose)
            _previousWindow.Show();
        base.OnClosing(e);
    }
}