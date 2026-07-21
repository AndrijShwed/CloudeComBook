using Avalonia.Controls;
using Avalonia.Interactivity;
using ClaudeComBook.Desktop.Models;
using ClaudeComBook.Desktop.Services;

namespace ClaudeComBook.Desktop.Views;

public partial class UsersManagementView : Window
{
    private readonly Window _previousWindow;
    private readonly ApiService _api = new();
    private bool _manualClose = false;

    public UsersManagementView(Window previousWindow)
    {
        InitializeComponent();
        _previousWindow = previousWindow;
        LoadData();
    }

    public async void LoadData()
    {
        var users = await _api.GetUsersAsync();
        UsersGrid.ItemsSource = users;
    }

    private void OnAddUserClick(object sender, RoutedEventArgs e)
    {
        var window = new AddUserView(this);
        window.Show();
        this.Hide();
    }

    private async void OnToggleActiveClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is UserModel user)
        {
            if (user.Login == AppSession.CurrentUser?.Login)
            {
                var err = MsBox.Avalonia.MessageBoxManager
                    .GetMessageBoxStandard("Помилка", "Не можна заблокувати себе!");
                await err.ShowAsync();
                return;
            }

            await _api.ToggleUserActiveAsync(user.Id, !user.IsActive);
            LoadData();
        }
    }

    private async void OnDeleteUserClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is UserModel user)
        {
            if (user.Login == AppSession.CurrentUser?.Login)
            {
                var err = MsBox.Avalonia.MessageBoxManager
                    .GetMessageBoxStandard("Помилка", "Не можна видалити себе!");
                await err.ShowAsync();
                return;
            }

            var msg = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Підтвердження",
                    $"Ви дійсно хочете видалити користувача \"{user.Login}\"?",
                    MsBox.Avalonia.Enums.ButtonEnum.YesNo);
            var result = await msg.ShowAsync();

            if (result == MsBox.Avalonia.Enums.ButtonResult.Yes)
            {
                await _api.DeleteUserAsync(user.Id);
                LoadData();
            }
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (!_manualClose)
            _previousWindow.Show();
        base.OnClosing(e);
    }
}