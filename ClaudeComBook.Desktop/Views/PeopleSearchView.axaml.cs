using Avalonia.Controls;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ClaudeComBook.Desktop.Models;
using ClaudeComBook.Desktop.Services;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ClaudeComBook.Desktop.Views;

public partial class PeopleSearchView : Window
{
    public readonly Window _previousWindow;
    private readonly ApiService _api = new();

    public PeopleSearchView(Window previousWindow)
    {
        InitializeComponent();
        UserLabel.Text = AppSession.CurrentUser?.FullName ?? AppSession.CurrentUser?.Login ?? "";
        LoadComboBoxes();
        AgeFromBox.AddHandler(TextInputEvent, OnAgeInput, RoutingStrategies.Tunnel);
        AgeToBox.AddHandler(TextInputEvent, OnAgeInput, RoutingStrategies.Tunnel);

        LastNameBox.TextChanged += OnNameTextChanged;
        FirstNameBox.TextChanged += OnNameTextChanged;
        SurnameBox.TextChanged += OnNameTextChanged;

        VillageBox.SelectionChanged += OnVillageChanged;
        StreetBox.SelectionChanged += OnStreetChanged;

        PeopleGrid.LoadingRow += OnLoadingRow;
        _previousWindow = previousWindow;
    }

    private void OnLoadingRow(object? sender, DataGridRowEventArgs e)
    {
        if (e.Row.DataContext is Person person)
        {
            if (person.Status?.Contains("помер", System.StringComparison.OrdinalIgnoreCase) == true)
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
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Distinct()
            .OrderBy(n => n, new HouseNumberComparer())
            .ToList();
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

        var numbers = await _api.GetStreetsAsync();
        StreetBox.ItemsSource = streets;
        StreetBox.DisplayMemberBinding = new Avalonia.Data.Binding("Name");

    }

    public async void OnSearchClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        int ageFrom = 0, ageTo = 200;
        int? statusYear = null;

        if (!string.IsNullOrEmpty(StatusDateBox.Text))
        {
            if (int.TryParse(StatusDateBox.Text, out var year) && year >= 1900 & year <= 2100)
                statusYear = year;
        }
                

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
            villageId: selectedVillage?.Id,
            streetId: selectedStreet?.Id,
            houseNumb: string.IsNullOrEmpty(HouseNumberBox.Text) ? null : HouseNumberBox.Text,
            ageFrom: string.IsNullOrEmpty(AgeFromBox.Text) ? null : ageFrom,
            ageTo: string.IsNullOrEmpty(AgeToBox.Text) ? null : ageTo,
            statusYear: statusYear
        );

        PeopleGrid.ItemsSource = people;
        ResultCount.Text = people?.Count.ToString() ?? "0";
    }

    private void OnClearClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        LastNameBox.Text = "";
        FirstNameBox.Text = "";
        SurnameBox.Text = "";
        HouseNumberBox.SelectedIndex = -1;
        AgeFromBox.Text = "";
        AgeToBox.Text = "";
        SexBox.SelectedIndex = -1;
        VillageBox.SelectedIndex = -1;
        StreetBox.SelectedIndex = -1;
        StatusBox.SelectedIndex = -1;
        RegistrYesBox.IsChecked = true;
        RegistrNoBox.IsChecked = false;
        StatusDateBox.Text = "";
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

    private void OnRowDoubleTapped(object sender, Avalonia.Input.TappedEventArgs e)
    {
        if (!AppSession.IsUser) return; // Reader не може редагувати

        if (PeopleGrid.SelectedItem is Person person)
        {
            var window = new PersonEditView(person, this);
            window.Show();
            this.Hide();
        }
    }

    private bool _manualClose = false;
    private void OnHomeClick(object sender, Avalonia.Input.TappedEventArgs e)
    {
        _manualClose = true;
        if (_previousWindow is PeopleView peopleView)
        {
            peopleView._previousWindow.Show();
            peopleView.Hide();
        }
        else
        {
            _previousWindow.Show();
        }
        Close();
    }

    private void OnPeopleViewClick(object sender, Avalonia.Input.TappedEventArgs e)
    {
        _manualClose = true;
        if (_previousWindow is PeopleSearchView peopleSearchView)
        {
            peopleSearchView._previousWindow.Show();
            peopleSearchView.Hide();
        }
        else
        {
            _previousWindow.Show();
        }
        Close();
    }


    private void OnPeopleClick(object sender, Avalonia.Input.TappedEventArgs e)
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