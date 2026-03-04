using System.Windows.Input;
using Wpf.Ui.Controls;

namespace Prosologic.Studio.Views.Dialogs;

public partial class InputDialog : FluentWindow
{
    public string? InputValue { get; private set; }

    public InputDialog(string title, string prompt, string placeholder, string defaultValue)
    {
        InitializeComponent();
        Title = title;
        PromptText.Text = prompt;
        InputBox.PlaceholderText = placeholder;
        InputBox.Text = defaultValue;

        Loaded += (_, _) =>
        {
            InputBox.Focus();
            InputBox.SelectAll();
        };
    }

    private void OK_Click(object sender, System.Windows.RoutedEventArgs e) => Accept();
    private void Cancel_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void InputBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Enter) { Accept(); e.Handled = true; }
        if (e.Key == Key.Escape) { DialogResult = false; Close(); e.Handled = true; }
    }

    private void Accept()
    {
        InputValue = InputBox.Text;
        DialogResult = true;
        Close();
    }
}