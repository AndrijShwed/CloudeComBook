using Avalonia.Controls;
using Avalonia.Interactivity;
using ClaudeComBook.Desktop.Models;
using ClaudeComBook.Desktop.Services;
using System.Linq;
using System.Text.RegularExpressions;

namespace ClaudeComBook.Desktop.Views;

public partial class EnterpriseSearchView : Window
{
    private readonly Window _previousWindow;
    private readonly ApiService _api = new();
    private bool _manualClose = false;

    public EnterpriseSearchView(Window previousWindow)
    {
        InitializeComponent();
        _previousWindow = previousWindow;
        LoadComboBoxes();
        VillageBox.SelectionChanged += OnVillageChanged;
        StreetBox.SelectionChanged += OnStreetChanged;
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

    private async void OnStreetChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (VillageBox.SelectedItem is not Village village) return;
        if (StreetBox.SelectedItem is not Street street) return;

        var villageStreets = await _api.GetVillageStreetsAsync();
        var vs = villageStreets?.FirstOrDefault(v =>
            v.VillageId == village.Id && v.StreetId == street.Id);

        if (vs == null) return;

        var houses = await _api.GetHousesByVillageStreetAsync(vs.Id);
        var numbers = houses?
           .Select(h => h.NumbOfHouse)
           .Distinct()
           .OrderBy(n =>
           {
               var match = Regex.Match(n ?? "", @"^\d+");
               return match.Success ? int.Parse(match.Value) : int.MaxValue;
           })
           .ThenBy(n => n)
           .ToList();
        HouseNumberBox.ItemsSource = numbers;
        HouseNumberBox.SelectedIndex = -1;
    }
    public async void OnSearchClick(object? sender, RoutedEventArgs? e)
    {
        var selectedVillage = VillageBox.SelectedItem as Village;
        var selectedStreet = StreetBox.SelectedItem as Street;

        var enterprises = await _api.SearchEnterprisesAsync(
            name: string.IsNullOrEmpty(NameBox.Text) ? null : NameBox.Text,
            owner: string.IsNullOrEmpty(OwnerBox.Text) ? null : OwnerBox.Text,
            villageId: selectedVillage?.Id,
            streetId: selectedStreet?.Id,
            houseNumber: string.IsNullOrEmpty(HouseNumberBox.Text) ? null : HouseNumberBox.Text);

        EnterprisesGrid.ItemsSource = enterprises;
        ResultCount.Text = enterprises?.Count.ToString() ?? "0";
    }

    private void OnClearClick(object sender, RoutedEventArgs e)
    {
        NameBox.Text = "";
        OwnerBox.Text = "";
        VillageBox.SelectedIndex = -1;
        StreetBox.SelectedIndex = -1;
        HouseNumberBox.Text = "";
    }

    private void OnClearTableClick(object sender, RoutedEventArgs e)
    {
        EnterprisesGrid.ItemsSource = null;
        ResultCount.Text = "0";
    }

    private async void OnDeleteClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is Enterprise enterprise)
        {
            var msg = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Підтвердження",
                    $"Ви дійсно хочете видалити підприємство \"{enterprise.Name}\"?",
                    MsBox.Avalonia.Enums.ButtonEnum.YesNo);
            var result = await msg.ShowAsync();

            if (result == MsBox.Avalonia.Enums.ButtonResult.Yes)
            {
                await _api.DeleteEnterpriseAsync(enterprise.Id);
                OnSearchClick(null, null);
            }
        }
    }

    private void OnRowDoubleTapped(object sender, Avalonia.Input.TappedEventArgs e)
    {
        if (EnterprisesGrid.SelectedItem is Enterprise enterprise)
        {
            var window = new EnterpriseEditView(enterprise, this);
            window.Show();
            this.Hide();
        }
    }

    private void OnHomeClick(object sender, Avalonia.Input.TappedEventArgs e)
    {
        _manualClose = true;
        if (_previousWindow is EnterprisesView enterprisesView)
            enterprisesView._previousWindow.Show();
        _previousWindow.Close();
        Close();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (!_manualClose)
            _previousWindow.Show();
        base.OnClosing(e);
    }
}