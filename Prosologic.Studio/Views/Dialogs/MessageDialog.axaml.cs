using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Prosologic.Studio.Views.Dialogs;

public partial class MessageDialog : Window, INotifyPropertyChanged
{
    private string _dialogTitle = "Message";
    private string _message = string.Empty;
    private string _icon = "i";
    private IBrush _iconBackground = new SolidColorBrush(Color.Parse("#0E639C"));
    private string _primaryButtonText = "OK";
    private string _secondaryButtonText = "No";
    private string _closeButtonText = "Cancel";
    private bool _showPrimaryButton = true;
    private bool _showSecondaryButton = false;
    private bool _showCloseButton = false;

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public string DialogTitle
    {
        get => _dialogTitle;
        set
        {
            _dialogTitle = value;
            OnPropertyChanged();
        }
    }

    public string Message
    {
        get => _message;
        set
        {
            _message = value;
            OnPropertyChanged();
        }
    }

    public string Icon
    {
        get => _icon;
        set
        {
            _icon = value;
            OnPropertyChanged();
        }
    }

    public IBrush IconBackground
    {
        get => _iconBackground;
        set
        {
            _iconBackground = value;
            OnPropertyChanged();
        }
    }

    public string PrimaryButtonText
    {
        get => _primaryButtonText;
        set
        {
            _primaryButtonText = value;
            OnPropertyChanged();
        }
    }

    public string SecondaryButtonText
    {
        get => _secondaryButtonText;
        set
        {
            _secondaryButtonText = value;
            OnPropertyChanged();
        }
    }

    public string CloseButtonText
    {
        get => _closeButtonText;
        set
        {
            _closeButtonText = value;
            OnPropertyChanged();
        }
    }

    public bool ShowPrimaryButton
    {
        get => _showPrimaryButton;
        set
        {
            _showPrimaryButton = value;
            OnPropertyChanged();
        }
    }

    public bool ShowSecondaryButton
    {
        get => _showSecondaryButton;
        set
        {
            _showSecondaryButton = value;
            OnPropertyChanged();
        }
    }

    public bool ShowCloseButton
    {
        get => _showCloseButton;
        set
        {
            _showCloseButton = value;
            OnPropertyChanged();
        }
    }

    public MessageDialog()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void PrimaryButton_Click(object? sender, RoutedEventArgs e)
    {
        Close(MessageDialogResult.Primary);
    }

    private void SecondaryButton_Click(object? sender, RoutedEventArgs e)
    {
        Close(MessageDialogResult.Secondary);
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        Close(MessageDialogResult.Cancel);
    }
}

public enum MessageDialogResult
{
    Primary,
    Secondary,
    Cancel
}

public enum MessageDialogType
{
    Information,
    Warning,
    Error,
    Success,
    Question
}

public enum MessageDialogButtons
{
    OK,
    OKCancel,
    YesNo,
    YesNoCancel
}