using System.Threading.Tasks;
using System.Windows;

namespace Prosologic.Studio.Services;

/// <summary>
/// WPF replacement for the Avalonia MessageBoxService.
/// Covers every method called by MainWindowViewModel:
///   ShowAsync, ShowErrorAsync, ShowWarningAsync, ConfirmAsync, ShowInputAsync.
/// </summary>
public class MessageBoxService
{
    private readonly Window _owner;

    public MessageBoxService(Window owner)
    {
        _owner = owner;
    }

    public Task ShowAsync(string title, string message)
    {
        System.Windows.MessageBox.Show(_owner, message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        return Task.CompletedTask;
    }

    public Task ShowErrorAsync(string title, string message)
    {
        System.Windows.MessageBox.Show(_owner, message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        return Task.CompletedTask;
    }

    public Task ShowWarningAsync(string title, string message)
    {
        System.Windows.MessageBox.Show(_owner, message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        return Task.CompletedTask;
    }

    public Task<bool> ConfirmAsync(string title, string message)
    {
        var result = System.Windows.MessageBox.Show(_owner, message, title,
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        return Task.FromResult(result == MessageBoxResult.Yes);
    }

    /// <summary>
    /// Shows a simple text-input dialog.
    /// Returns the entered text, or null/empty if the user cancelled.
    /// Uses the InputDialog view in Views/Dialogs — see InputDialog.xaml.
    /// </summary>
    public Task<string?> ShowInputAsync(string title, string prompt, string placeholder, string defaultValue)
    {
        var dialog = new Views.Dialogs.InputDialog(title, prompt, placeholder, defaultValue)
        {
            Owner = _owner
        };

        bool? result = dialog.ShowDialog();
        return Task.FromResult(result == true ? dialog.InputValue : (string?)null);
    }
}