using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace Prosologic.Studio.Services;

/// <summary>
/// Folder picker using Microsoft.Win32.OpenFolderDialog,
/// available natively in WPF on .NET 8+ — no extra packages or
/// System.Windows.Forms reference needed.
/// </summary>
public class FileDialogService
{
    private readonly Window _owner;

    public FileDialogService(Window owner)
    {
        _owner = owner;
    }

    /// <summary>Prompts the user to select an existing folder. Returns null if cancelled.</summary>
    public Task<string?> PickOpenFolderAsync()
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Select project folder"
        };

        return Task.FromResult(dialog.ShowDialog(_owner) == true ? dialog.FolderName : null);
    }

    /// <summary>Prompts the user to select a folder to save into. Returns null if cancelled.</summary>
    public Task<string?> PickSaveFolderAsync(string suggestedName = "")
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Choose folder to save project into"
        };

        if (dialog.ShowDialog(_owner) != true) return Task.FromResult((string?)null);

        // Append the project name as a sub-folder, mirroring the original behaviour.
        var target = string.IsNullOrWhiteSpace(suggestedName)
            ? dialog.FolderName
            : Path.Combine(dialog.FolderName, suggestedName);

        return Task.FromResult<string?>(target);
    }
}