using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ClaudeComBook.Desktop.Models;
using ClaudeComBook.Desktop.Services;
using System;
using System.Linq;

namespace ClaudeComBook.Desktop.Views;

public partial class PersonEditView : Window
{
    private readonly Window _previousWindow;
    private readonly ApiService _api = new();
    private readonly Person _person;
    private readonly bool _isAddMode = false;

    public PersonEditView(Person person, Window previousWindow)
    {
        InitializeComponent();
        _person = person;
        _previousWindow = previousWindow;
        MDateBox.AddHandler(TextInputEvent, OnDateInput, RoutingStrategies.Tunnel);
        MDateBox.TextChanged += OnMDateTextChanged;
        DateOfBirthBox.AddHandler(TextInputEvent, OnDateInput, RoutingStrategies.Tunnel);
        DateOfBirthBox.TextChanged += OnDateOfBirthTextChanged;
        LoadData();
        DeleteBtn.IsVisible = AppSession.IsAdmin;
    }

    // Режим додавання
    public PersonEditView(Window previousWindow)
    {
        InitializeComponent();
        _person = new Person();
        _previousWindow = previousWindow;
        _isAddMode = true;
        MDateBox.AddHandler(TextInputEvent, OnDateInput, RoutingStrategies.Tunnel);
        MDateBox.TextChanged += OnMDateTextChanged;
        DateOfBirthBox.AddHandler(TextInputEvent, OnDateInput, RoutingStrategies.Tunnel);
        DateOfBirthBox.TextChanged += OnDateOfBirthTextChanged;
        LoadData();
    }
    private void OnDateInput(object? sender, TextInputEventArgs e)
    {
        if (!char.IsDigit(e.Text![0]))
            e.Handled = true;
    }

    private void OnMDateTextChanged(object? sender, TextChangedEventArgs e)
    {
        ApplyDateMask(MDateBox);
    }

    private void OnDateOfBirthTextChanged(object? sender, TextChangedEventArgs e)
    {
        ApplyDateMask(DateOfBirthBox);
    }

    private bool _isApplyingMask = false;

    private void ApplyDateMask(TextBox box)
    {
        if (_isApplyingMask) return;
        _isApplyingMask = true;

        var text = box.Text ?? "";
        var digits = new string(text.Where(char.IsDigit).ToArray());

        if (digits.Length >= 2)
            digits = digits.Insert(2, ".");
        if (digits.Length >= 5)
            digits = digits.Insert(5, ".");
        if (digits.Length > 10)
            digits = digits.Substring(0, 10);

        box.Text = digits;
        box.CaretIndex = box.Text.Length;

        _isApplyingMask = false;
    }
    private async void LoadData()
    {
        Title = _isAddMode ? "Додавання особи" : "Редагувати";
        // Завантажуємо населені пункти
        var villages = await _api.GetVillagesAsync();
        VillageBox.ItemsSource = villages;
        VillageBox.DisplayMemberBinding = new Avalonia.Data.Binding("Name");

        // Заповнюємо поля
        LastNameBox.Text = _person.LastName;
        FirstNameBox.Text = _person.Name;
        SurnameBox.Text = _person.Surname;
        HouseBox.Text = _person.NumbOfHouse;
        PassportBox.Text = _person.Passport;
        IdKodBox.Text = _person.IdKod;
        PhoneBox.Text = _person.PhoneNumber;
        MilIDBox.Text = _person.MilID;
        DescriptionBox.Text = _person.Description;

        if (_person.DateOfBirth.HasValue)
            DateOfBirthBox.Text = _person.DateOfBirth.Value.ToString("dd.MM.yyyy");

        if (_person.MDate.HasValue)
            MDateBox.Text = _person.MDate.Value.ToString("dd.MM.yyyy");

        // Стать
        if (_isAddMode)
            SexBox.SelectedIndex = 0;
        else
            SexBox.SelectedIndex = _person.Sex == "чол" ? 0 : 1;

        // Реєстрація
        if (_isAddMode)
            RegistrBox.SelectedIndex = 0;
        else
            RegistrBox.SelectedIndex = _person.Registr == "так" ? 0 : 1;

        // Статус
        var statusItems = StatusBox.Items.Cast<ComboBoxItem>().ToList();
        var statusItem = statusItems.FirstOrDefault(i =>
            i.Content?.ToString() == _person.Status);
        if (statusItem != null)
            StatusBox.SelectedItem = statusItem;

        // Населений пункт
        if (_person.VillageName != null && villages != null)
        {
            var village = villages.FirstOrDefault(v => v.Name == _person.VillageName);
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

        // Вибираємо поточну вулицю
        if (_person.StreetName != null && filteredStreets != null)
        {
            var street = filteredStreets.FirstOrDefault(s => s.Name == _person.StreetName);
            if (street != null)
                StreetBox.SelectedItem = street;
        }
    }

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
        // Перевірка обов'язкових полів
        if (string.IsNullOrEmpty(LastNameBox.Text))
        {
            var err = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Помилка", "Прізвище є обов'язковим полем!");
            await err.ShowAsync();
            return;
        }
        if (string.IsNullOrEmpty(FirstNameBox.Text))
        {
            var err = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Помилка", "Ім'я є обов'язковим полем!");
            await err.ShowAsync();
            return;
        }
        if (VillageBox.SelectedItem == null)
        {
            var err = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Помилка", "Населений пункт є обов'язковим полем!");
            await err.ShowAsync();
            return;
        }
        if (StreetBox.SelectedItem == null)
        {
            var err = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Помилка", "Вулиця є обов'язковим полем!");
            await err.ShowAsync();
            return;
        }
        if (string.IsNullOrEmpty(HouseBox.Text))
        {
            var err = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Помилка", "Номер будинку є обов'язковим полем!");
            await err.ShowAsync();
            return;
        }

        if (_isAddMode)
        {
            DateTime? dobirth = null;
            if (System.DateTime.TryParseExact(DateOfBirthBox.Text, "dd.MM.yyyy",
                null, System.Globalization.DateTimeStyles.None, out var dobParsed))
                dobirth = dobParsed;

            var exists = await _api.PersonExistsAsync(
                LastNameBox.Text!, FirstNameBox.Text!, SurnameBox.Text, dobirth);

            if (exists)
            {
                var confirm = MsBox.Avalonia.MessageBoxManager
                    .GetMessageBoxStandard("Увага",
                        $"Особа {LastNameBox.Text} {FirstNameBox.Text} {SurnameBox.Text} вже існує!\nВи бажаєте все одно додати?",
                        MsBox.Avalonia.Enums.ButtonEnum.YesNo);
                var result = await confirm.ShowAsync();
                if (result != MsBox.Avalonia.Enums.ButtonResult.Yes)
                    return;
            }
        }

        // Отримуємо villageStreetId
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

        _person.LastName = LastNameBox.Text;
        _person.Name = FirstNameBox.Text;
        _person.Surname = SurnameBox.Text;
        _person.Sex = (SexBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
        _person.Registr = (RegistrBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
        _person.NumbOfHouse = HouseBox.Text;
        _person.Passport = PassportBox.Text;
        _person.IdKod = IdKodBox.Text;
        _person.PhoneNumber = PhoneBox.Text;
        _person.MilID = MilIDBox.Text;
        _person.Description = DescriptionBox.Text;
        _person.VillageStreetId = villageStreetId;
        _person.Status = (StatusBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

        if (System.DateTime.TryParseExact(DateOfBirthBox.Text, "dd.MM.yyyy",
            null, System.Globalization.DateTimeStyles.None, out var dob))
            _person.DateOfBirth = dob;

        if (System.DateTime.TryParseExact(MDateBox.Text, "dd.MM.yyyy",
            null, System.Globalization.DateTimeStyles.None, out var mdate))
            _person.MDate = mdate;

        if (_isAddMode)
            await _api.CreatePersonAsync(_person);
        else
            await _api.UpdatePersonAsync(_person);

        var msg = MsBox.Avalonia.MessageBoxManager
            .GetMessageBoxStandard("Успіх", _isAddMode ? "Особу додано!" : "Зміни збережено!");
        
        await msg.ShowAsync();

        _previousWindow.Show();
        Close();
    }

    private void OnBackClick(object sender, RoutedEventArgs e)
    {
        _previousWindow.Show();
        Close();
    }
    private async void OnDeleteClick(object sender, RoutedEventArgs e)
    {
        var msg = MsBox.Avalonia.MessageBoxManager
            .GetMessageBoxStandard("Підтвердження",
                $"Ви дійсно хочете видалити особу \"{_person.LastName} {_person.Name}\"?",
                MsBox.Avalonia.Enums.ButtonEnum.YesNo);
        var result = await msg.ShowAsync();

        if (result == MsBox.Avalonia.Enums.ButtonResult.Yes)
        {
            await _api.DeletePersonAsync(_person.PeopleId);
            if (_previousWindow is PeopleSearchView searchView)
                searchView.OnSearchClick(null!, null!);
            _previousWindow.Show();
            Close();
        }
    }
    
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        _previousWindow.Show();
        base.OnClosing(e);
    }

    private bool _manualClose = false;
    private void OnSearchWindowClick(object sender, Avalonia.Input.TappedEventArgs e)
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
  
}