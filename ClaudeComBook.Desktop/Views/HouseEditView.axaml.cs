using Avalonia.Controls;
using Avalonia.Interactivity;
using ClaudeComBook.Desktop.Models;
using ClaudeComBook.Desktop.Services;
using System.Linq;

namespace ClaudeComBook.Desktop.Views;

public partial class HouseEditView : Window
{
    private readonly Window _previousWindow;
    private readonly ApiService _api = new();
    private readonly House _house;
    private bool _manualClose = false;

    public HouseEditView(House house, Window previousWindow)
    {
        InitializeComponent();
        UserLabel.Text = AppSession.CurrentUser?.FullName ?? AppSession.CurrentUser?.Login ?? "";
        _house = house;
        _previousWindow = previousWindow;
        LoadData();
        DeleteBtn.IsVisible = AppSession.IsAdmin;
    }

    private async void LoadData()
    {
        // Завантажуємо населені пункти
        var villages = await _api.GetVillagesAsync();
        VillageBox.ItemsSource = villages;
        VillageBox.DisplayMemberBinding = new Avalonia.Data.Binding("Name");

        // Заповнюємо поля
        HouseNumberBox.Text = _house.NumbOfHouse;
        LastNameBox.Text = _house.LastName;
        FirstNameBox.Text = _house.Name;
        SurnameBox.Text = _house.Surname;
        TotalAreaBox.Text = _house.TotalArea?.ToString();
        LivingAreaBox.Text = _house.LivingArea?.ToString();
        RoomsBox.Text = _house.TotalOfRooms?.ToString();

        // Вибираємо населений пункт
        if (_house.VillageName != null && villages != null)
        {
            var village = villages.FirstOrDefault(v => v.Name == _house.VillageName);
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

        if (_house.StreetName != null && filteredStreets != null)
        {
            var street = filteredStreets.FirstOrDefault(s => s.Name == _house.StreetName);
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

        _house.VillageStreetId = villageStreetId ?? _house.VillageStreetId;
        _house.NumbOfHouse = HouseNumberBox.Text;
        _house.LastName = LastNameBox.Text;
        _house.Name = FirstNameBox.Text;
        _house.Surname = SurnameBox.Text;
        _house.TotalArea = decimal.TryParse(TotalAreaBox.Text, out var ta) ? ta : null;
        _house.LivingArea = decimal.TryParse(LivingAreaBox.Text, out var la) ? la : null;
        _house.TotalOfRooms = int.TryParse(RoomsBox.Text, out var rooms) ? rooms : null;

        await _api.UpdateHouseAsync(_house);

        var msg = MsBox.Avalonia.MessageBoxManager
            .GetMessageBoxStandard("Успіх", "Зміни збережено!");
        await msg.ShowAsync();

        if (_previousWindow is HouseSearchView searchView)
            searchView.OnSearchClick(null, null);

        _previousWindow.Show();
        Close();
    }

    private async void OnDeleteClick(object sender, RoutedEventArgs e)
    {
        var msg = MsBox.Avalonia.MessageBoxManager
            .GetMessageBoxStandard("Підтвердження",
                $"Ви дійсно хочете видалити будинок \"{_house.NumbOfHouse}\" на вулиці \"{_house.StreetName}\"?",
                MsBox.Avalonia.Enums.ButtonEnum.YesNo);
        var result = await msg.ShowAsync();

        if (result == MsBox.Avalonia.Enums.ButtonResult.Yes)
        {
            await _api.DeleteHouseAsync(_house.IdHouses);
            if (_previousWindow is HouseSearchView searchView)
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