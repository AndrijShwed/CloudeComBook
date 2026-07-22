using Avalonia.Controls;
using Avalonia.Interactivity;
using ClaudeComBook.Desktop.Models;
using ClaudeComBook.Desktop.Services;
using MsBox.Avalonia.Base;

namespace ClaudeComBook.Desktop.Views;

public partial class AddAnymalView : Window
{
    private readonly Window _previousWindow;
    private readonly ApiService _api = new();
    private bool _manualClose = false;

    public AddAnymalView(Window previousWindow)
    {
        InitializeComponent();
        UserLabel.Text = AppSession.CurrentUser?.FullName ?? AppSession.CurrentUser?.Login ?? "";
        _previousWindow = previousWindow;
        LoadData();
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
        if (string.IsNullOrEmpty(LastNameBox.Text))
        {
            var err = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Помилка", "Введіть прізвище!");
            await err.ShowAsync();
            return;
        }
        if (string.IsNullOrEmpty(FirstNameBox.Text))
        {
            var err = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Помилка", "Введіть ім'я!");
            await err.ShowAsync();
            return;
        }
        var selectedVillage = VillageBox.SelectedItem as Village;

        if (selectedVillage == null)
        {
            var err = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Помилка", "Виберіть населений пункт!");
            await err.ShowAsync();
            return;
        }

        // Перевірка дублювання
        var exists = await _api.AnymalExistsAsync(
            LastNameBox.Text, FirstNameBox.Text, SurnameBox.Text, selectedVillage.Name);

        if (exists)
        {
            var confirm = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Увага",
                    $"Запис для {LastNameBox.Text} {FirstNameBox.Text} {SurnameBox.Text} " +
                    $"з {selectedVillage.Name} вже існує!\nВи бажаєте все одно додати?",
                    MsBox.Avalonia.Enums.ButtonEnum.YesNo);
            var result = await confirm.ShowAsync();
            if (result != MsBox.Avalonia.Enums.ButtonResult.Yes)
                return;
        }

        var animal = new Anymal
        {
            LastName = LastNameBox.Text,
            Name = FirstNameBox.Text,
            Surname = SurnameBox.Text,
            Village = selectedVillage.Name,
            Anymals = int.TryParse(AnymalsBox.Text, out var anymals) ? anymals : 0,
            Covs = int.TryParse(CovsBox.Text, out var covs) ? covs : 0,
            Pigs = int.TryParse(PigsBox.Text, out var pigs) ? pigs : 0,
            Sheeps = int.TryParse(SheepsBox.Text, out var sheeps) ? sheeps : 0,
            Goats = int.TryParse(GoatsBox.Text, out var goats) ? goats : 0,
            Horses = int.TryParse(HorsesBox.Text, out var horses) ? horses : 0,
            Birds = int.TryParse(BirdsBox.Text, out var birds) ? birds : 0,
            Rabbits = int.TryParse(RabbitsBox.Text, out var rabbits) ? rabbits : 0,
            Beeses = int.TryParse(BeesesBox.Text, out var beeses) ? beeses : 0
        };

        await _api.CreateAnymalAsync(animal);

        var msg = MsBox.Avalonia.MessageBoxManager
            .GetMessageBoxStandard("Успіх", "Запис додано!");
        await msg.ShowAsync();

        // Очищаємо поля
        LastNameBox.Text = "";
        FirstNameBox.Text = "";
        SurnameBox.Text = "";
        VillageBox.Text = "";
        AnymalsBox.Text = "";
        CovsBox.Text = "";
        PigsBox.Text = "";
        SheepsBox.Text = "";
        GoatsBox.Text = "";
        HorsesBox.Text = "";
        BirdsBox.Text = "";
        RabbitsBox.Text = "";
        BeesesBox.Text = "";
    }

    private void OnHomeClick(object sender, Avalonia.Input.TappedEventArgs e)
    {
        _manualClose = true;
        if (_previousWindow is AnymalsView anymalsView)
            anymalsView._previousWindow.Show();
        _previousWindow.Close();
        Close();
    }

    private void OnAnymalsClick(object sender, Avalonia.Input.TappedEventArgs e)
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