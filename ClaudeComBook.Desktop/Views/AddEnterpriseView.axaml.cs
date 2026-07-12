using Avalonia.Controls;
using Avalonia.Interactivity;
using ClaudeComBook.Desktop.Models;
using ClaudeComBook.Desktop.Services;
using System.Linq;

namespace ClaudeComBook.Desktop.Views;

public partial class AddEnterpriseView : Window
{
    private readonly Window _previousWindow;
    private readonly ApiService _api = new();
    private bool _manualClose = false;

    public AddEnterpriseView(Window previousWindow)
    {
        InitializeComponent();
        _previousWindow = previousWindow;
        LoadData();
        VillageBox.SelectionChanged += OnVillageChanged;
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

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(NameBox.Text))
        {
            var err = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Помилка", "Введіть назву підприємства!");
            await err.ShowAsync();
            return;
        }
        var exists = await _api.EnterpriseExistsByNameAsync(NameBox.Text);
        if (exists)
        {
            var err = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Помилка",
                    $"Підприємство з назвою \"{NameBox.Text}\" вже існує!");
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
        if (StreetBox.SelectedItem == null)
        {
            var err = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Помилка", "Виберіть вулицю!");
            await err.ShowAsync();
            return;
        }
        if (string.IsNullOrEmpty(OwnerBox.Text))
        {
            var err = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Помилка", "Введіть ПІП власника!");
            await err.ShowAsync();
            return;
        }

        var selectedVillage = VillageBox.SelectedItem as Village;
        var selectedStreet = StreetBox.SelectedItem as Street;

        int? villageStreetId = null;
        if (selectedVillage != null && selectedStreet != null)
        {
            var villageStreets = await _api.GetVillageStreetsAsync();
            var vs = villageStreets?.FirstOrDefault(v =>
                v.VillageId == selectedVillage.Id && v.StreetId == selectedStreet.Id);
            villageStreetId = vs?.Id;
        }

        var enterprise = new Enterprise
        {
            Name = NameBox.Text,
            EmployeesNumber = int.TryParse(EmployeesBox.Text, out var emp) ? emp : null,
            Owner = OwnerBox.Text,
            HouseNumber = HouseNumberBox.Text,
            VillageStreetId = villageStreetId
        };

        await _api.CreateEnterpriseAsync(enterprise);

        var msg = MsBox.Avalonia.MessageBoxManager
            .GetMessageBoxStandard("Успіх", "Підприємство додано!");
        await msg.ShowAsync();

        // Очищаємо поля
        VillageBox.SelectedIndex = -1;
        StreetBox.SelectedIndex = -1;
        HouseNumberBox.Text = "";
        NameBox.Text = "";
        EmployeesBox.Text = "";
        OwnerBox.Text = "";
    }

    private void OnHomeClick(object sender, Avalonia.Input.TappedEventArgs e)
    {
        _manualClose = true;
        if (_previousWindow is EnterprisesView enterprisesView)
            enterprisesView._previousWindow.Show();
        _previousWindow.Close();
        Close();
    }

    private void OnEnterprisesClick(object sender, Avalonia.Input.TappedEventArgs e)
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