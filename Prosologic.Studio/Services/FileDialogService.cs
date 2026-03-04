using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Threading.Tasks;

namespace Prosologic.Studio.Services
{
    public class FileDialogService
    {
        private readonly Window _parentWindow;

        public FileDialogService(Window parentWindow)
        {
            _parentWindow = parentWindow;
        }

        public async Task<string?> PickSaveFolderAsync(string suggestedName)
        {
            var folder = await _parentWindow.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Save Project",
                AllowMultiple = false,
                SuggestedFileName = suggestedName
            });

            if (folder.Count > 0)
            {
                return folder[0].Path.LocalPath;
            }

            return null;
        }

        public async Task<string?> PickOpenFolderAsync()
        {
            var folder = await _parentWindow.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Open Project",
                AllowMultiple = false
            });

            if (folder.Count > 0)
            {
                return folder[0].Path.LocalPath;
            }

            return null;
        }
    }
}
