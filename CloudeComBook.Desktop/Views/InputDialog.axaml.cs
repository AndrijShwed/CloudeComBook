using Avalonia.Controls;
using Avalonia.Interactivity;

namespace CloudeComBook.Desktop.Views;

public partial class InputDialog : Window
{
    public string? Result { get; private set; }

    public InputDialog(string message, string defaultValue = "")
    {
        InitializeComponent();
        MessageText.Text = message;
        InputBox.Text = defaultValue;
    }

    private void OnOkClick(object sender, RoutedEventArgs e)
    {
        Result = InputBox.Text;
        Close();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        Result = null;
        Close();
    }
}