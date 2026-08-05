using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ClaudeComBook.Desktop.Views;

public partial class TestamentDialog : Window
{
    public string? PlaceOfBirth { get; private set; }
    public string? FullNameTestamentPerson { get; private set; }
    public string? DateOfBirthTestamentPerson { get; private set; }
    public string? TestamentNumber { get; private set; }

    public TestamentDialog(
        string message,
        string placeOfBirth = "",
        string fullNameTestamentPerson = "",
        string dateOfBirthTestamentPerson = "",
        string testamentNumber = "")
    {
        InitializeComponent();

        MessageText.Text = message;

        InputBoxTestamentPlaceOfBirth.Text = placeOfBirth;
        InputBoxTestamentFullName.Text = fullNameTestamentPerson;
        InputBoxTestamentDateOfBirth.Text = dateOfBirthTestamentPerson;
        InputBoxTestamentNumber.Text = testamentNumber;
    }

    private void OnOkClick(object sender, RoutedEventArgs e)
    {
        PlaceOfBirth = InputBoxTestamentPlaceOfBirth.Text?.Trim();
        FullNameTestamentPerson = InputBoxTestamentFullName.Text?.Trim();
        DateOfBirthTestamentPerson = InputBoxTestamentDateOfBirth.Text?.Trim();
        TestamentNumber = InputBoxTestamentNumber.Text?.Trim();

        Close(true);
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        Close(false);
    }
}