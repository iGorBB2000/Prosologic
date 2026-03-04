using CommunityToolkit.Mvvm.Input;
using Prosologic.Core.Models;

namespace Prosologic.Studio.ViewModels;

public class TagGroupEditorViewModel : ViewModelBase
{
    private TagGroup? _currentGroup;
    private bool _hasChanges;

    private string _name = string.Empty;
    private string _description = string.Empty;

    public string Name
    {
        get => _name;
        set { SetProperty(ref _name, value); HasChanges = true; }
    }

    public string Description
    {
        get => _description;
        set { SetProperty(ref _description, value); HasChanges = true; }
    }

    public bool HasChanges
    {
        get => _hasChanges;
        private set
        {
            SetProperty(ref _hasChanges, value);
            SaveCommand.NotifyCanExecuteChanged();
            ResetCommand.NotifyCanExecuteChanged();
        }
    }

    public bool IsGroupLoaded => _currentGroup != null;

    public RelayCommand SaveCommand { get; }
    public RelayCommand ResetCommand { get; }

    public event EventHandler<(TagGroup Group, string OldName, string NewName)>? GroupNameChanged;
    public event EventHandler? GroupSaved;

    public TagGroupEditorViewModel()
    {
        SaveCommand = new RelayCommand(Save, () => IsGroupLoaded && HasChanges);
        ResetCommand = new RelayCommand(Reset, () => IsGroupLoaded);
    }

    public void LoadGroup(TagGroup group)
    {
        _currentGroup = group;
        _name = group.Name;
        _description = group.Description ?? string.Empty;

        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(Description));
        OnPropertyChanged(nameof(IsGroupLoaded));

        HasChanges = false;
    }

    public void Clear()
    {
        _currentGroup = null;
        _name = string.Empty;
        _description = string.Empty;

        OnPropertyChanged(nameof(IsGroupLoaded));
        HasChanges = false;
    }

    private void Save()
    {
        if (_currentGroup == null) return;

        var oldName = _currentGroup.Name;

        _currentGroup.Name = Name;
        _currentGroup.Description = string.IsNullOrWhiteSpace(Description) ? null : Description;

        HasChanges = false;

        if (oldName != Name)
            GroupNameChanged?.Invoke(this, (_currentGroup, oldName, Name));

        GroupSaved?.Invoke(this, EventArgs.Empty);
    }

    private void Reset()
    {
        if (_currentGroup != null) LoadGroup(_currentGroup);
    }
}