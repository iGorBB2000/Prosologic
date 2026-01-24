using Avalonia.Controls;
using Prosologic.Studio.Views.Dialogs;
using System.Threading.Tasks;

namespace Prosologic.Studio.Services
{
    public class MessageBoxService
    {
        private readonly Window _parentWindow;

        public MessageBoxService(Window parentWindow)
        {
            _parentWindow = parentWindow;
        }

        public async Task<bool> ConfirmAsync(string title, string message)
        {
            var dialog = MessageDialogBuilder.Create(
                title,
                message,
                MessageDialogType.Question,
                MessageDialogButtons.YesNo
            );

            var result = await dialog.ShowDialog<MessageDialogResult>(_parentWindow);
            return result == MessageDialogResult.Primary; // Primary = Yes
        }

        public async Task<string?> ShowInputAsync(string title, string prompt, string placeholder = "", string defaultValue = "")
        {
            var dialog = new InputDialog
            {
                DialogTitle = title,
                Prompt = prompt,
                Placeholder = placeholder,
                InputValue = defaultValue
            };

            return await dialog.ShowDialog<string?>(_parentWindow);
        }

        public async Task ShowErrorAsync(string title, string message)
        {
            var dialog = MessageDialogBuilder.Create(
                title,
                message,
                MessageDialogType.Error,
                MessageDialogButtons.OK
            );

            await dialog.ShowDialog(_parentWindow);
        }

        public async Task ShowInfoAsync(string title, string message)
        {
            var dialog = MessageDialogBuilder.Create(
                title,
                message,
                MessageDialogType.Information,
                MessageDialogButtons.OK
            );

            await dialog.ShowDialog(_parentWindow);
        }

        public async Task ShowWarningAsync(string title, string message)
        {
            var dialog = MessageDialogBuilder.Create(
                title,
                message,
                MessageDialogType.Warning,
                MessageDialogButtons.OK
            );

            await dialog.ShowDialog(_parentWindow);
        }

        public async Task ShowSuccessAsync(string title, string message)
        {
            var dialog = MessageDialogBuilder.Create(
                title,
                message,
                MessageDialogType.Success,
                MessageDialogButtons.OK
            );

            await dialog.ShowDialog(_parentWindow);
        }

        public async Task<MessageDialogResult> ConfirmWithCancelAsync(string title, string message)
        {
            var dialog = MessageDialogBuilder.Create(
                title,
                message,
                MessageDialogType.Question,
                MessageDialogButtons.YesNoCancel
            );

            return await dialog.ShowDialog<MessageDialogResult>(_parentWindow);
        }

        public async Task<MessageDialogResult> ShowCustomAsync(
            string title,
            string message,
            MessageDialogType type,
            MessageDialogButtons buttons)
        {
            var dialog = MessageDialogBuilder.Create(title, message, type, buttons);
            return await dialog.ShowDialog<MessageDialogResult>(_parentWindow);
        }
    }
}
