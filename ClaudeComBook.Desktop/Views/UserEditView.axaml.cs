using Avalonia.Controls;
using Avalonia.Interactivity;
using ClaudeComBook.Desktop.Models;
using ClaudeComBook.Desktop.Services;
using System.Linq;

namespace ClaudeComBook.Desktop.Views;

public partial class UserEditView : Window
{
    private readonly UserModel _user;
    private readonly UsersManagementView _previousWindow;
    private readonly ApiService _api = new();
    private bool _showPassword = false;

    public UserEditView(UserModel user, UsersManagementView previousWindow)
    {
        InitializeComponent();
        _user = user;
        _previousWindow = previousWindow;
        LoadData();
    }

    private void LoadData()
    {
        LoginBox.Text = _user.Login;
        FullNameBox.Text = _user.FullName;

        var roleItems = RoleBox.Items.Cast<ComboBoxItem>().ToList();
        var roleItem = roleItems.FirstOrDefault(i => i.Content?.ToString() == _user.Role);
        if (roleItem != null)
            RoleBox.SelectedItem = roleItem;
    }

    private void OnShowPasswordClick(object sender, RoutedEventArgs e)
    {
        _showPassword = !_showPassword;
        PasswordBox.PasswordChar = _showPassword ? '\0' : '*';
        ShowPasswordBtn.Content = _showPassword ? "🙈" : "👁";
    }

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(LoginBox.Text))
        {
            var err = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Помилка", "Введіть логін!");
            await err.ShowAsync();
            return;
        }

        var selectedRole = (RoleBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

        var result = await _api.UpdateUserAsync(
            _user.Id,
            LoginBox.Text,
            FullNameBox.Text,
            selectedRole ?? _user.Role,
            string.IsNullOrEmpty(PasswordBox.Text) ? null : PasswordBox.Text);

        if (result)
        {
            var msg = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Успіх", "Зміни збережено!");
            await msg.ShowAsync();
            _previousWindow.LoadData();
            Close();
        }
        else
        {
            var err = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Помилка", "Помилка збереження!");
            await err.ShowAsync();
        }
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}