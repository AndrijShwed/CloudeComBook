using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ClaudeComBook.Desktop.Models;
using ClaudeComBook.Desktop.Services;
using System.Threading.Tasks;
using System.Linq;
using Avalonia.Controls;

namespace ClaudeComBook.Desktop.Views;

public partial class PeopleSearchView : Window
{
    private readonly ApiService _api = new();

    public PeopleSearchView()
    {
        InitializeComponent();
        LoadComboBoxes();
        AgeFromBox.AddHandler(TextInputEvent, OnAgeInput, RoutingStrategies.Tunnel);
        AgeToBox.AddHandler(TextInputEvent, OnAgeInput, RoutingStrategies.Tunnel);

        LastNameBox.TextChanged += OnNameTextChanged;
        FirstNameBox.TextChanged += OnNameTextChanged;
        SurnameBox.TextChanged += OnNameTextChanged;

        VillageBox.SelectionChanged += OnVillageChanged;

        PeopleGrid.LoadingRow += OnLoadingRow;
    }

    private void OnLoadingRow(object? sender, DataGridRowEventArgs e)
    {
        if (e.Row.DataContext is Person person)
        {
            if (person.Status?.ToLower() == "помер" || person.Status?.ToLower() == "померла")
            {
                e.Row.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Black);
                e.Row.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.White);
            }
            else if (person.Registr?.ToLower() == "ні")
            {
                e.Row.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(255, 182, 193));
                e.Row.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Black);
            }
            else
            {
                e.Row.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(245, 245, 220));
                e.Row.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Black);
            }
        }
    }

    private async void OnVillageChanged(object? sender, SelectionChangedEventArgs e)
    {
        var selectedVillage = VillageBox.SelectedItem as Village;
        if (selectedVillage == null)
        {
            StreetBox.ItemsSource = null;
            StreetBox.SelectedIndex = -1;
            return;
        }

        var villageStreets = await _api.GetVillageStreetsAsync();
        var streetIds = villageStreets?
            .Where(vs => vs.VillageId == selectedVillage.Id && vs.IsActive)
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

    private void OnAgeInput(object sender, TextInputEventArgs e)
    {
        e.Handled = !int.TryParse(e.Text, out _);
    }

    private async void LoadComboBoxes()
    {
        var villages = await _api.GetVillagesAsync();
        VillageBox.ItemsSource = villages;
        VillageBox.DisplayMemberBinding = new Avalonia.Data.Binding("Name");

        var streets = await _api.GetStreetsAsync();
        StreetBox.ItemsSource = streets;
        StreetBox.DisplayMemberBinding = new Avalonia.Data.Binding("Name");
    }

    private async void OnSearchClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        int ageFrom = 0, ageTo = 200;

        if (!string.IsNullOrEmpty(AgeFromBox.Text))
        {
            if (!int.TryParse(AgeFromBox.Text, out ageFrom) || ageFrom > 200)
            {
                AgeFromBox.Text = "";
                var msg = MsBox.Avalonia.MessageBoxManager
                    .GetMessageBoxStandard("Помилка", "Вік від: має бути від 0 до 200");
                await msg.ShowAsync();
                return;
            }
        }

        if (!string.IsNullOrEmpty(AgeToBox.Text))
        {
            if (!int.TryParse(AgeToBox.Text, out ageTo) || ageTo > 200)
            {
                AgeToBox.Text = "";
                var msg = MsBox.Avalonia.MessageBoxManager
                    .GetMessageBoxStandard("Помилка", "Вік до: має бути від 0 до 200");
                await msg.ShowAsync();
                return;
            }
        }

        if (!string.IsNullOrEmpty(AgeFromBox.Text) &&
            !string.IsNullOrEmpty(AgeToBox.Text) && ageFrom > ageTo)
        {
            AgeFromBox.Text = "";
            AgeToBox.Text = "";
            var msg = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Помилка", "Вік від: не може бути більше ніж Вік до:");
            await msg.ShowAsync();
            return;
        }

        // Отримуємо вибрані значення
        var selectedVillage = VillageBox.SelectedItem as Village;
        var selectedStreet = StreetBox.SelectedItem as Street;
        var selectedSex = (SexBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
        var registr = (RegistrYesBox.IsChecked == true && RegistrNoBox.IsChecked == true) ? null :
              RegistrYesBox.IsChecked == true ? "так" :
              RegistrNoBox.IsChecked == true ? "ні" : null;

        var people = await _api.GetPeopleAsync(
            lastName: string.IsNullOrEmpty(LastNameBox.Text) ? null : LastNameBox.Text,
            name: string.IsNullOrEmpty(FirstNameBox.Text) ? null : FirstNameBox.Text,
            surname: string.IsNullOrEmpty(SurnameBox.Text) ? null : SurnameBox.Text,
            sex: selectedSex,
            status: (StatusBox.SelectedItem as ComboBoxItem)?.Content?.ToString(),
            registr: registr,
            villageStreetId: selectedVillage != null && selectedStreet != null
                ? await GetVillageStreetId(selectedVillage.Id, selectedStreet.Id)
                : null,
            houseNumb: string.IsNullOrEmpty(HouseBox.Text) ? null : HouseBox.Text,
            ageFrom: string.IsNullOrEmpty(AgeFromBox.Text) ? null : ageFrom,
            ageTo: string.IsNullOrEmpty(AgeToBox.Text) ? null : ageTo
        );

        PeopleGrid.ItemsSource = people;
        ResultCount.Text = people?.Count.ToString() ?? "0";
    }

    private async Task<int?> GetVillageStreetId(int villageId, int streetId)
    {
        var villageStreets = await _api.GetVillageStreetsAsync();
        return villageStreets?
            .FirstOrDefault(vs => vs.VillageId == villageId && vs.StreetId == streetId)?.Id;
    }

    private void OnClearClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        LastNameBox.Text = "";
        FirstNameBox.Text = "";
        SurnameBox.Text = "";
        HouseBox.Text = "";
        AgeFromBox.Text = "";
        AgeToBox.Text = "";
        SexBox.SelectedIndex = -1;
        VillageBox.SelectedIndex = -1;
        StreetBox.SelectedIndex = -1;
        StatusBox.SelectedIndex = -1;
        RegistrYesBox.IsChecked = true;
        RegistrNoBox.IsChecked = false;
    }

    private void OnClearTableClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        PeopleGrid.ItemsSource = null;
        ResultCount.Text = "0";
    }

    private void OnExcelClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Пізніше додамо експорт в Excel
    }
}