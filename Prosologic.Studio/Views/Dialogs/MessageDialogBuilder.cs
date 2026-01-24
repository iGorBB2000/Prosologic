using Avalonia.Media;

namespace Prosologic.Studio.Views.Dialogs
{
    public static class MessageDialogBuilder
    {
        public static MessageDialog Create(
            string title,
            string message,
            MessageDialogType type = MessageDialogType.Information,
            MessageDialogButtons buttons = MessageDialogButtons.OK)
        {
            var dialog = new MessageDialog
            {
                DialogTitle = title,
                Message = message
            };

            switch (type)
            {
                case MessageDialogType.Information:
                    dialog.Icon = "i";
                    dialog.IconBackground = new SolidColorBrush(Color.Parse("#0E639C"));
                    break;

                case MessageDialogType.Warning:
                    dialog.Icon = "!";
                    dialog.IconBackground = new SolidColorBrush(Color.Parse("#FFA500"));
                    break;

                case MessageDialogType.Error:
                    dialog.Icon = "x";
                    dialog.IconBackground = new SolidColorBrush(Color.Parse("#DC3545"));
                    break;

                case MessageDialogType.Success:
                    dialog.Icon = "v";
                    dialog.IconBackground = new SolidColorBrush(Color.Parse("#28A745"));
                    break;

                case MessageDialogType.Question:
                    dialog.Icon = "?";
                    dialog.IconBackground = new SolidColorBrush(Color.Parse("#6C757D"));
                    break;
            }

            switch (buttons)
            {
                case MessageDialogButtons.OK:
                    dialog.PrimaryButtonText = "OK";
                    dialog.ShowPrimaryButton = true;
                    dialog.ShowSecondaryButton = false;
                    dialog.ShowCloseButton = false;
                    break;

                case MessageDialogButtons.OKCancel:
                    dialog.PrimaryButtonText = "OK";
                    dialog.CloseButtonText = "Cancel";
                    dialog.ShowPrimaryButton = true;
                    dialog.ShowSecondaryButton = false;
                    dialog.ShowCloseButton = true;
                    break;

                case MessageDialogButtons.YesNo:
                    dialog.PrimaryButtonText = "Yes";
                    dialog.SecondaryButtonText = "No";
                    dialog.ShowPrimaryButton = true;
                    dialog.ShowSecondaryButton = true;
                    dialog.ShowCloseButton = false;
                    break;

                case MessageDialogButtons.YesNoCancel:
                    dialog.PrimaryButtonText = "Yes";
                    dialog.SecondaryButtonText = "No";
                    dialog.CloseButtonText = "Cancel";
                    dialog.ShowPrimaryButton = true;
                    dialog.ShowSecondaryButton = true;
                    dialog.ShowCloseButton = true;
                    break;
            }

            return dialog;
        }
    }
}
