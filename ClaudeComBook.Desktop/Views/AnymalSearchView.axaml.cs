using Avalonia.Controls;
using Avalonia.Interactivity;
using ClaudeComBook.Desktop.Models;
using ClaudeComBook.Desktop.Services;
using System.Linq;

namespace ClaudeComBook.Desktop.Views;

public partial class AnymalSearchView : Window
{
    private readonly Window _previousWindow;
    private readonly ApiService _api = new();
    private bool _manualClose = false;

    public AnymalSearchView(Window previousWindow)
    {
        InitializeComponent();
        UserLabel.Text = AppSession.CurrentUser?.FullName ?? AppSession.CurrentUser?.Login ?? "";
        _previousWindow = previousWindow;
        LoadComboBoxes();
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

        var anymals = await _api.SearchAnymalsAsync(
            lastName: string.IsNullOrEmpty(LastNameBox.Text) ? null : LastNameBox.Text,
            name: string.IsNullOrEmpty(FirstNameBox.Text) ? null : FirstNameBox.Text,
            surname: string.IsNullOrEmpty(SurnameBox.Text) ? null : SurnameBox.Text,
            village: selectedVillage?.Name);

        // Фільтруємо по чекбоксах
        if (anymals != null)
        {
            var filtered = anymals.AsEnumerable();
            if (CovsCheck.IsChecked == true) filtered = filtered.Where(a => a.Covs > 0);
            if (HorsesCheck.IsChecked == true) filtered = filtered.Where(a => a.Horses > 0);
            if (PigsCheck.IsChecked == true) filtered = filtered.Where(a => a.Pigs > 0);
            if (SheepsCheck.IsChecked == true) filtered = filtered.Where(a => a.Sheeps > 0);
            if (GoatsCheck.IsChecked == true) filtered = filtered.Where(a => a.Goats > 0);
            if (BirdsCheck.IsChecked == true) filtered = filtered.Where(a => a.Birds > 0);
            if (RabbitsCheck.IsChecked == true) filtered = filtered.Where(a => a.Rabbits > 0);
            if (BeesesCheck.IsChecked == true) filtered = filtered.Where(a => a.Beeses > 0);

            var result = filtered.ToList();
            AnymalsGrid.ItemsSource = result;
            ResultCount.Text = result.Count.ToString();
        }
    }

    private void OnClearClick(object sender, RoutedEventArgs e)
    {
        LastNameBox.Text = "";
        FirstNameBox.Text = "";
        SurnameBox.Text = "";
        VillageBox.SelectedIndex = -1;
        CovsCheck.IsChecked = false;
        HorsesCheck.IsChecked = false;
        PigsCheck.IsChecked = false;
        SheepsCheck.IsChecked = false;
        GoatsCheck.IsChecked = false;
        BirdsCheck.IsChecked = false;
        RabbitsCheck.IsChecked = false;
        BeesesCheck.IsChecked = false;
    }

    private void OnClearTableClick(object sender, RoutedEventArgs e)
    {
        AnymalsGrid.ItemsSource = null;
        ResultCount.Text = "0";
    }

    private async void OnDeleteClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is Anymal animal)
        {
            var msg = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Підтвердження",
                    $"Ви дійсно хочете видалити запис \"{animal.LastName} {animal.Name}\"?",
                    MsBox.Avalonia.Enums.ButtonEnum.YesNo);
            var result = await msg.ShowAsync();

            if (result == MsBox.Avalonia.Enums.ButtonResult.Yes)
            {
                await _api.DeleteAnymalAsync(animal.AnymalsId);
                OnSearchClick(null, null);
            }
        }
    }

    private void OnRowDoubleTapped(object sender, Avalonia.Input.TappedEventArgs e)
    {
        if (!AppSession.IsUser) return;
        if (AnymalsGrid.SelectedItem is Anymal anymal)
        {
            var window = new AnymalEditView(anymal, this);
            window.Show();
            this.Hide();
        }
    }

    private void OnHomeClick(object sender, Avalonia.Input.TappedEventArgs e)
    {
        _manualClose = true;
        if (_previousWindow is AnymalsView animalsView)
            animalsView._previousWindow.Show();
        _previousWindow.Close();
        Close();
    }

    private void OnAnimalsClick(object sender, Avalonia.Input.TappedEventArgs e)
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