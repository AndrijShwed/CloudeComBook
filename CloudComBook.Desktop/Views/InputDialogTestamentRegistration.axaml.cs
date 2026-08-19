using Avalonia.Controls;
using Avalonia.Interactivity;

namespace CloudComBook.Desktop.Views;

public partial class InputDialogTestamentRegistration : Window
{
    public string? TestamentNumber { get; private set; }
    public string? PlaceOfBirth { get; private set; }
    public string? PostalCode { get; private set; }
    public string? RegistrationDate { get; private set; }

    public InputDialogTestamentRegistration(
        string message,
        string testamentNumber = "",
        string placeOfBirth = "",
        string postalCode = "",
        string registrationDate = "")
    {
        InitializeComponent();

        MessageText.Text = message;

        InputBoxTestamentNumber.Text = testamentNumber;
        InputBoxTestamentPlaceOfBirth.Text = placeOfBirth;
        InputBoxTestamentPostalCode.Text = postalCode;
        InputBoxTestamentRegistrationDate.Text = registrationDate;
    }

    private void OnOkClick(object sender, RoutedEventArgs e)
    {
        TestamentNumber = InputBoxTestamentNumber.Text?.Trim();
        PlaceOfBirth = InputBoxTestamentPlaceOfBirth.Text?.Trim();
        PostalCode = InputBoxTestamentPostalCode.Text?.Trim();
        RegistrationDate = InputBoxTestamentRegistrationDate.Text?.Trim();

        Close(true);
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        Close(false);
    }
}