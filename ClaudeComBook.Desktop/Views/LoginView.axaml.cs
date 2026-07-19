using Avalonia.Controls;
using Avalonia.Interactivity;
using ClaudeComBook.Desktop.Services;

namespace ClaudeComBook.Desktop.Views;

public partial class LoginView : Window
{
    private readonly ApiService _api = new();

    public LoginView()
    {
        InitializeComponent();
    }

    private async void OnLoginClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(LoginBox.Text) || string.IsNullOrEmpty(PasswordBox.Text))
        {
            ErrorText.Text = "Введіть логін і пароль!";
            ErrorText.IsVisible = true;
            return;
        }

        var result = await _api.LoginAsync(LoginBox.Text, PasswordBox.Text);

        if (result == null)
        {
            ErrorText.Text = "Невірний логін або пароль!";
            ErrorText.IsVisible = true;
            return;
        }

        // Зберігаємо сесію
        AppSession.CurrentUser = result;

        // Відкриваємо головне вікно
        var mainWindow = new MainWindow();
        mainWindow.Show();
        Close();
    }
    private bool _showPassword = false;

    private void OnShowPasswordClick(object sender, RoutedEventArgs e)
    {
        _showPassword = !_showPassword;
        PasswordBox.PasswordChar = _showPassword ? '\0' : '*';
        ShowPasswordBtn.Content = _showPassword ? "🙈" : "👁";
    }
}