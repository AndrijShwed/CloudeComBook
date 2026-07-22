using Avalonia.Controls;
using Avalonia.Interactivity;
using ClaudeComBook.Desktop.Models;
using ClaudeComBook.Desktop.Services;
using System.Linq;

namespace ClaudeComBook.Desktop.Views;

public partial class LandEditView : Window
{
    private readonly Window _previousWindow;
    private readonly ApiService _api = new();
    private readonly Plot _plot;
    private bool _manualClose = false;

    public LandEditView(Plot plot, Window previousWindow)
    {
        InitializeComponent();
        UserLabel.Text = AppSession.CurrentUser?.FullName ?? AppSession.CurrentUser?.Login ?? "";
        _plot = plot;
        _previousWindow = previousWindow;
        LoadData();
        DeleteBtn.IsVisible = AppSession.IsAdmin;
        CadastrBox.TextChanged += OnCadastrChanged;
    }

    private async void LoadData()
    {
        var villages = await _api.GetVillagesAsync();
        VillageBox.ItemsSource = villages;
        VillageBox.DisplayMemberBinding = new Avalonia.Data.Binding("Name");

        FullNameBox.Text = _plot.FullName;
        HouseNumberBox.Text = _plot.HouseNumb;
        PlotTypeBox.Text = _plot.PlotType;
        FieldNumberBox.Text = _plot.FieldNumber;
        PlotNumberBox.Text = _plot.PlotNumber;
        PlotAreaBox.Text = _plot.PlotArea?.ToString();
        CadastrBox.Text = _plot.Cadastr;
        TenantBox.Text = _plot.Tenant;
        UrlBox.Text = _plot.Url;

        if (_plot.Village != null && villages != null)
        {
            var village = villages.FirstOrDefault(v => v.Name == _plot.Village);
            if (village != null)
            {
                VillageBox.SelectedItem = village;
                await LoadStreets(village.Id);
            }
        }

        VillageBox.SelectionChanged += OnVillageChanged;
    }

    private async void OnVillageChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (VillageBox.SelectedItem is Village village)
            await LoadStreets(village.Id);
    }

    private async System.Threading.Tasks.Task LoadStreets(int villageId)
    {
        var villageStreets = await _api.GetVillageStreetsAsync();
        var streetIds = villageStreets?
            .Where(vs => vs.VillageId == villageId && vs.IsActive)
            .Select(vs => vs.StreetId)
            .ToList();

        var allStreets = await _api.GetStreetsAsync();
        var filteredStreets = allStreets?
            .Where(s => streetIds != null && streetIds.Contains(s.Id))
            .ToList();

        StreetBox.ItemsSource = filteredStreets;
        StreetBox.DisplayMemberBinding = new Avalonia.Data.Binding("Name");

        if (_plot.Street != null && filteredStreets != null)
        {
            var street = filteredStreets.FirstOrDefault(s => s.Name == _plot.Street);
            if (street != null)
                StreetBox.SelectedItem = street;
        }
    }

    private bool _isFormattingCadastr = false;
    private void OnCadastrChanged(object? sender, TextChangedEventArgs e)
    {
        if (_isFormattingCadastr) return;
        _isFormattingCadastr = true;

        var text = CadastrBox.Text ?? "";
        var digits = new string(text.Where(c => char.IsDigit(c)).ToArray());

        var formatted = "";
        if (digits.Length > 0) formatted = digits.Substring(0, System.Math.Min(10, digits.Length));
        if (digits.Length > 10) formatted += ":" + digits.Substring(10, System.Math.Min(2, digits.Length - 10));
        if (digits.Length > 12) formatted += ":" + digits.Substring(12, System.Math.Min(3, digits.Length - 12));
        if (digits.Length > 15) formatted += ":" + digits.Substring(15, System.Math.Min(4, digits.Length - 15));

        CadastrBox.Text = formatted;
        CadastrBox.CaretIndex = formatted.Length;
        _isFormattingCadastr = false;
    }

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
        var selectedVillage = VillageBox.SelectedItem as Village;
        var selectedStreet = StreetBox.SelectedItem as Street;

        _plot.FullName = FullNameBox.Text;
        _plot.Village = selectedVillage?.Name ?? _plot.Village;
        _plot.Street = selectedStreet?.Name ?? _plot.Street;
        _plot.HouseNumb = HouseNumberBox.Text;
        _plot.PlotType = PlotTypeBox.Text;
        _plot.FieldNumber = FieldNumberBox.Text;
        _plot.PlotNumber = PlotNumberBox.Text;
        _plot.PlotArea = decimal.TryParse(PlotAreaBox.Text, out var area) ? area : null;
        _plot.Cadastr = CadastrBox.Text;
        _plot.Tenant = TenantBox.Text;
        _plot.Url = UrlBox.Text;

        await _api.UpdatePlotAsync(_plot);

        var msg = MsBox.Avalonia.MessageBoxManager
            .GetMessageBoxStandard("Успіх", "Зміни збережено!");
        await msg.ShowAsync();

        if (_previousWindow is LandSearchView searchView)
            searchView.OnSearchClick(null, null);

        _previousWindow.Show();
        Close();
    }

    private async void OnDeleteClick(object sender, RoutedEventArgs e)
    {
        var msg = MsBox.Avalonia.MessageBoxManager
            .GetMessageBoxStandard("Підтвердження",
                $"Ви дійсно хочете видалити ділянку \"{_plot.FullName}\"?",
                MsBox.Avalonia.Enums.ButtonEnum.YesNo);
        var result = await msg.ShowAsync();

        if (result == MsBox.Avalonia.Enums.ButtonResult.Yes)
        {
            await _api.DeletePlotAsync(_plot.Id);
            if (_previousWindow is LandSearchView searchView)
                searchView.OnSearchClick(null, null);
            _previousWindow.Show();
            Close();
        }
    }

    private void OnBackClick(object sender, RoutedEventArgs e)
    {
        _previousWindow.Show();
        Close();
    }

    private void OnBackClick2(object sender, Avalonia.Input.TappedEventArgs e)
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