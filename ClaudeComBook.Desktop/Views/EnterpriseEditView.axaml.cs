using Avalonia.Controls;
using Avalonia.Interactivity;
using ClaudeComBook.Desktop.Models;
using ClaudeComBook.Desktop.Services;
using System.Linq;

namespace ClaudeComBook.Desktop.Views;

public partial class EnterpriseEditView : Window
{
    private readonly Window _previousWindow;
    private readonly ApiService _api = new();
    private readonly Enterprise _enterprise;
    private bool _manualClose = false;

    public EnterpriseEditView(Enterprise enterprise, Window previousWindow)
    {
        InitializeComponent();
        UserLabel.Text = AppSession.CurrentUser?.FullName ?? AppSession.CurrentUser?.Login ?? "";
        _enterprise = enterprise;
        _previousWindow = previousWindow;
        LoadData();
        DeleteBtn.IsVisible = AppSession.IsAdmin;
    }

    private async void LoadData()
    {
        var villages = await _api.GetVillagesAsync();
        VillageBox.ItemsSource = villages;
        VillageBox.DisplayMemberBinding = new Avalonia.Data.Binding("Name");

        NameBox.Text = _enterprise.Name;
        EmployeesBox.Text = _enterprise.EmployeesNumber?.ToString();
        OwnerBox.Text = _enterprise.Owner;
        HouseNumberBox.Text = _enterprise.HouseNumber;

        if (_enterprise.VillageName != null && villages != null)
        {
            var village = villages.FirstOrDefault(v => v.Name == _enterprise.VillageName);
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

        if (_enterprise.StreetName != null && filteredStreets != null)
        {
            var street = filteredStreets.FirstOrDefault(s => s.Name == _enterprise.StreetName);
            if (street != null)
                StreetBox.SelectedItem = street;
        }
    }

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
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

        _enterprise.Name = NameBox.Text;
        _enterprise.EmployeesNumber = int.TryParse(EmployeesBox.Text, out var emp) ? emp : null;
        _enterprise.Owner = OwnerBox.Text;
        _enterprise.HouseNumber = HouseNumberBox.Text;
        _enterprise.VillageStreetId = villageStreetId ?? _enterprise.VillageStreetId;

        await _api.UpdateEnterpriseAsync(_enterprise);

        var msg = MsBox.Avalonia.MessageBoxManager
            .GetMessageBoxStandard("Успіх", "Зміни збережено!");
        await msg.ShowAsync();

        if (_previousWindow is EnterpriseSearchView searchView)
            searchView.OnSearchClick(null, null);

        _previousWindow.Show();
        Close();
    }

    private async void OnDeleteClick(object sender, RoutedEventArgs e)
    {
        var msg = MsBox.Avalonia.MessageBoxManager
            .GetMessageBoxStandard("Підтвердження",
                $"Ви дійсно хочете видалити підприємство \"{_enterprise.Name}\"?",
                MsBox.Avalonia.Enums.ButtonEnum.YesNo);
        var result = await msg.ShowAsync();

        if (result == MsBox.Avalonia.Enums.ButtonResult.Yes)
        {
            await _api.DeleteEnterpriseAsync(_enterprise.Id);
            if (_previousWindow is EnterpriseSearchView searchView)
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