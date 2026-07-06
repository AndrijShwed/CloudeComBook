using Avalonia.Controls;
using Avalonia.Interactivity;
using ClaudeComBook.Desktop.Models;
using ClaudeComBook.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClaudeComBook.Desktop.Views;

public partial class HouseSearchView : Window
{
    private readonly Window _previousWindow;
    private readonly ApiService _api = new();
    private bool _manualClose = false;

    public HouseSearchView(Window previousWindow)
    {
        InitializeComponent();
        _previousWindow = previousWindow;
        LoadComboBoxes();
        VillageBox.SelectionChanged += OnVillageChanged;
        StreetBox.SelectionChanged += OnStreetChanged;
        LastNameBox.TextChanged += OnNameTextChanged;
        FirstNameBox.TextChanged += OnNameTextChanged;
        SurnameBox.TextChanged += OnNameTextChanged;
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
        HouseNumberBox.ItemsSource = null;
    }

    private async void OnStreetChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (VillageBox.SelectedItem is not Village village) return;
        if (StreetBox.SelectedItem is not Street street) return;

        var villageStreets = await _api.GetVillageStreetsAsync();
        var vs = villageStreets?.FirstOrDefault(v =>
            v.VillageId == village.Id && v.StreetId == street.Id);

        if (vs == null) return;

        var houses = await _api.GetHousesByVillageStreetAsync(vs.Id);
        var numbers = houses?.Select(h => h.NumbOfHouse).Distinct().OrderBy(n => n).ToList();
        HouseNumberBox.ItemsSource = numbers;
        HouseNumberBox.SelectedIndex = -1;
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
        var houseNumber = HouseNumberBox.SelectedItem as string;

        var houses = await _api.SearchHousesAsync(
            villageId: selectedVillage?.Id,
            streetId: selectedStreet?.Id,
            houseNumber: houseNumber,
            lastName: string.IsNullOrEmpty(LastNameBox.Text) ? null : LastNameBox.Text,
            name: string.IsNullOrEmpty(FirstNameBox.Text) ? null : FirstNameBox.Text,
            surname: string.IsNullOrEmpty(SurnameBox.Text) ? null : SurnameBox.Text);

        HousesGrid.ItemsSource = houses;
        CountText.Text = houses?.Count.ToString() ?? "0";
    }

    private void OnClearClick(object sender, RoutedEventArgs e)
    {
        VillageBox.SelectedIndex = -1;
        StreetBox.SelectedIndex = -1;
        HouseNumberBox.SelectedIndex = -1;
        LastNameBox.Text = "";
        FirstNameBox.Text = "";
        SurnameBox.Text = "";
        HousesGrid.ItemsSource = null;
        CountText.Text = "0";
    }

    private async void OnDeleteClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is House house)
        {
            var msg = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Підтвердження",
                    $"Ви дійсно хочете видалити будинок \"{house.NumbOfHouse}\" на вулиці \"{house.StreetName}\"?",
                    MsBox.Avalonia.Enums.ButtonEnum.YesNo);
            var result = await msg.ShowAsync();

            if (result == MsBox.Avalonia.Enums.ButtonResult.Yes)
            {
                await _api.DeleteHouseAsync(house.IdHouses);
                OnSearchClick(null, null);
            }
        }
    }

    private void OnRowDoubleTapped(object sender, Avalonia.Input.TappedEventArgs e)
    {
        if (HousesGrid.SelectedItem is House house)
        {
            var window = new HouseEditView(house, this);
            window.Show();
            this.Hide();
        }
    }

    private void OnHomeClick(object sender, Avalonia.Input.TappedEventArgs e)
    {
        _manualClose = true;
        if (_previousWindow is HouseholdsView householdsView)
            householdsView._previousWindow.Show();
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