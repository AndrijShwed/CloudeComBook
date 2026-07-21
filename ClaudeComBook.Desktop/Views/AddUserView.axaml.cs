using Avalonia.Controls;
using Avalonia.Interactivity;
using ClaudeComBook.Desktop.Services;

namespace ClaudeComBook.Desktop.Views;

public partial class AddUserView : Window
{
    private readonly Window _previousWindow;
    private readonly ApiService _api = new();
    private bool _showPassword = false;

    public AddUserView(Window previousWindow)
    {
        InitializeComponent();
        _previousWindow = previousWindow;
        RoleBox.SelectedIndex = 1; // user за замовчуванням
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
            ErrorText.Text = "Введіть логін!";
            ErrorText.IsVisible = true;
            return;
        }
        if (string.IsNullOrEmpty(PasswordBox.Text))
        {
            ErrorText.Text = "Введіть пароль!";
            ErrorText.IsVisible = true;
            return;
        }
        if (RoleBox.SelectedItem == null)
        {
            ErrorText.Text = "Виберіть роль!";
            ErrorText.IsVisible = true;
            return;
        }

        var role = (RoleBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

        var result = await _api.RegisterUserAsync(
            LoginBox.Text,
            PasswordBox.Text,
            FullNameBox.Text,
            role);

        if (result == null)
        {
            ErrorText.Text = "Користувач з таким логіном вже існує!";
            ErrorText.IsVisible = true;
            return;
        }

        if (_previousWindow is UsersManagementView usersView)
            usersView.LoadData();

        _previousWindow.Show();
        Close();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        _previousWindow.Show();
        Close();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        _previousWindow.Show();
        base.OnClosing(e);
    }
}