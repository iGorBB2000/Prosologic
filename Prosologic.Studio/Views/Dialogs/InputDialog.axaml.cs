using Avalonia.Controls;
using Avalonia.Interactivity;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Prosologic.Studio.Views.Dialogs;

public partial class InputDialog : Window, INotifyPropertyChanged
{
    private string _dialogTitle = "Input";
    private string _prompt = "Enter value:";
    private string _placeholder = string.Empty;
    private string _inputValue = string.Empty;

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public string DialogTitle
    {
        get => _dialogTitle;
        set { _dialogTitle = value; OnPropertyChanged(); }
    }

    public string Prompt
    {
        get => _prompt;
        set { _prompt = value; OnPropertyChanged(); }
    }

    public string Placeholder
    {
        get => _placeholder;
        set { _placeholder = value; OnPropertyChanged(); }
    }

    public string InputValue
    {
        get => _inputValue;
        set { _inputValue = value; OnPropertyChanged(); }
    }

    public InputDialog()
    {
        InitializeComponent();
        DataContext = this;

        Opened += (s, e) => InputTextBox.Focus();
    }

    private void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        Close(InputValue);
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        Close(null);
    }
}