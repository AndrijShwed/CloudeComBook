using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ClaudeComBook.Desktop.Views;

public partial class InputDialog : Window
{
    public string? Result1 { get; private set; }
    public string? Result2 { get; private set; }
    public string? Result3 { get; private set; }
    public string? Result2_1 { get; private set; }
    public string? Result3_1 { get; private set; }

    public InputDialog(string message, string defaultValue = "")
    {
        InitializeComponent();
        MessageText.Text = message;
        InputBox1.Text = defaultValue;
        InputBox2.Text = defaultValue;
        InputBox3.Text = defaultValue;
        InputBox2_1.Text = defaultValue;
        InputBox3_1.Text = defaultValue;
        
    }

    private void OnOkClick(object sender, RoutedEventArgs e)
    {
        Result1 = InputBox1.Text;
        Result2 = InputBox2.Text;
        Result3 = InputBox3.Text;
        Result2_1 = InputBox2_1.Text;
        Result3_1 = InputBox3_1.Text;

        Close();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        Result1 = null;
        Result2 = null;
        Result3 = null;
        Result2_1 = null;
        Result3_1 = null;
        Close();
    }
}