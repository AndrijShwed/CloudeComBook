using Avalonia.Controls;
using Avalonia.Interactivity;
using ClaudeComBook.Desktop.Models;
using ClaudeComBook.Desktop.Services;
using System.Linq;

namespace ClaudeComBook.Desktop.Views;

public partial class AddLandView : Window
{
    private readonly Window _previousWindow;
    private readonly ApiService _api = new();
    private bool _manualClose = false;

    public AddLandView(Window previousWindow)
    {
        InitializeComponent();
        UserLabel.Text = AppSession.CurrentUser?.FullName ?? AppSession.CurrentUser?.Login ?? "";
        _previousWindow = previousWindow;
        LoadData();
        VillageBox.SelectionChanged += OnVillageChanged;
        FullNameBox.TextChanged += OnNameTextChanged;
        CadastrBox.TextChanged += OnCadastrChanged;
    }

    private async void LoadData()
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
        if (string.IsNullOrEmpty(FullNameBox.Text))
        {
            var err = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Помилка", "Введіть ПІБ власника!");
            await err.ShowAsync();
            return;
        }
        if (VillageBox.SelectedItem == null)
        {
            var err = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Помилка", "Виберіть населений пункт!");
            await err.ShowAsync();
            return;
        }

        var selectedVillage = VillageBox.SelectedItem as Village;
        var selectedStreet = StreetBox.SelectedItem as Street;

        var plot = new Plot
        {
            FullName = FullNameBox.Text,
            Village = selectedVillage?.Name,
            Street = selectedStreet?.Name,
            HouseNumb = HouseNumberBox.Text,
            PlotType = PlotTypeBox.Text,
            FieldNumber = FieldNumberBox.Text,
            PlotNumber = PlotNumberBox.Text,
            PlotArea = decimal.TryParse(PlotAreaBox.Text, out var area) ? area : null,
            Cadastr = CadastrBox.Text,
            Tenant = TenantBox.Text,
            Url = UrlBox.Text
        };

        await _api.CreatePlotAsync(plot);

        var msg = MsBox.Avalonia.MessageBoxManager
            .GetMessageBoxStandard("Успіх", "Земельну ділянку додано!");
        await msg.ShowAsync();

        // Очищаємо поля
        FullNameBox.Text = "";
        VillageBox.SelectedIndex = -1;
        StreetBox.SelectedIndex = -1;
        HouseNumberBox.Text = "";
        PlotTypeBox.Text = "";
        FieldNumberBox.Text = "";
        PlotNumberBox.Text = "";
        PlotAreaBox.Text = "";
        CadastrBox.Text = "";
        TenantBox.Text = "";
        UrlBox.Text = "";
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