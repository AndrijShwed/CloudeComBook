using Avalonia.Controls;
using Avalonia.Interactivity;
using ClaudeComBook.Desktop.Models;
using ClaudeComBook.Desktop.Services;
using System.Linq;

namespace ClaudeComBook.Desktop.Views;

public partial class AddHouseView : Window
{
    private readonly Window _previousWindow;
    private readonly ApiService _api = new();
    private bool _manualClose = false;

    public AddHouseView(Window previousWindow)
    {
        InitializeComponent();
        UserLabel.Text = AppSession.CurrentUser?.FullName ?? AppSession.CurrentUser?.Login ?? "";
        _previousWindow = previousWindow;
        LoadData();
        VillageBox.SelectionChanged += OnVillageChanged;
        LastNameBox.TextChanged += OnNameTextChanged;
        FirstNameBox.TextChanged += OnNameTextChanged;
        SurnameBox.TextChanged += OnNameTextChanged;
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

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
        // Перевірка обов'язкових полів
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
        if (string.IsNullOrEmpty(HouseNumberBox.Text))
        {
            var err = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Помилка", "Введіть номер будинку!");
            await err.ShowAsync();
            return;
        }
        if (string.IsNullOrEmpty(LastNameBox.Text))
        {
            var err = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Помилка", "Введіть прізвище власника!");
            await err.ShowAsync();
            return;
        }
        if (string.IsNullOrEmpty(FirstNameBox.Text))
        {
            var err = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Помилка", "Введіть ім'я власника!");
            await err.ShowAsync();
            return;
        }

        // Отримуємо villageStreetId
        var selectedVillage = VillageBox.SelectedItem as Village;
        var selectedStreet = StreetBox.SelectedItem as Street;

        var villageStreets = await _api.GetVillageStreetsAsync();
        var vs = villageStreets?.FirstOrDefault(v =>
            v.VillageId == selectedVillage!.Id && v.StreetId == selectedStreet!.Id);

        // Перевірка дублювання
        var exists = await _api.HouseExistsAsync(vs?.Id ?? 0, HouseNumberBox.Text);
        if (exists)
        {
            var err = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Помилка",
                    "Такий будинок вже існує у цьому населеному пункті на цій вулиці!");
            await err.ShowAsync();
            return;
        }

        var house = new House
        {
            VillageStreetId = vs?.Id ?? 0,
            NumbOfHouse = HouseNumberBox.Text,
            LastName = LastNameBox.Text,
            Name = FirstNameBox.Text,
            Surname = SurnameBox.Text,
            TotalArea = decimal.TryParse(TotalAreaBox.Text, out var ta) ? ta : null,
            LivingArea = decimal.TryParse(LivingAreaBox.Text, out var la) ? la : null,
            TotalOfRooms = int.TryParse(RoomsBox.Text, out var rooms) ? rooms : null
        };

        await _api.CreateHouseAsync(house);

        var msg = MsBox.Avalonia.MessageBoxManager
            .GetMessageBoxStandard("Успіх", "Будинок додано!");
        await msg.ShowAsync();

        // Очищаємо поля
        VillageBox.SelectedIndex = -1;
        StreetBox.SelectedIndex = -1;
        HouseNumberBox.Text = "";
        LastNameBox.Text = "";
        FirstNameBox.Text = "";
        SurnameBox.Text = "";
        TotalAreaBox.Text = "";
        LivingAreaBox.Text = "";
        RoomsBox.Text = "";
    }

    private void OnHomeClick(object sender, Avalonia.Input.TappedEventArgs e)
    {
        _manualClose = true;
        if (_previousWindow is HouseholdsView householdsView)
            householdsView._previousWindow.Show();
        else
            _previousWindow.Show();
        _previousWindow.Close();
        Close();
    }

    private void OnHouseholdsClick(object sender, Avalonia.Input.TappedEventArgs e)
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