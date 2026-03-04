using CommunityToolkit.Mvvm.Input;
using Prosologic.Core.Enums;
using Prosologic.Core.Models;

namespace Prosologic.Studio.ViewModels;

public class TagEditorViewModel : ViewModelBase
{
    private Tag? _currentTag;
    private bool _hasChanges;

    public event EventHandler? TagSaved;
    public event EventHandler<(Tag Tag, string OldName, string NewName, Action<bool> Callback)>? ValidateNameChange;

    #region Properties

    private string _name = string.Empty;
    private TagDataType _dataType;
    private UpdateStrategy _updateStrategy;
    private int _updateInterval = 1000;
    private string _initialValue = string.Empty;
    private string _engineeringUnit = string.Empty;
    private TagAccessMode _accessMode;
    private string _description = string.Empty;
    private string _scriptContent = string.Empty;

    public string Name
    {
        get => _name;
        set { SetProperty(ref _name, value); HasChanges = true; }
    }

    public TagDataType DataType
    {
        get => _dataType;
        set { SetProperty(ref _dataType, value); HasChanges = true; }
    }

    public UpdateStrategy UpdateStrategy
    {
        get => _updateStrategy;
        set
        {
            SetProperty(ref _updateStrategy, value);
            HasChanges = true;
            OnPropertyChanged(nameof(IsScriptRequired));
        }
    }

    public int UpdateInterval
    {
        get => _updateInterval;
        set { SetProperty(ref _updateInterval, value); HasChanges = true; }
    }

    public string InitialValue
    {
        get => _initialValue;
        set { SetProperty(ref _initialValue, value); HasChanges = true; }
    }

    public string EngineeringUnit
    {
        get => _engineeringUnit;
        set { SetProperty(ref _engineeringUnit, value); HasChanges = true; }
    }

    public TagAccessMode AccessMode
    {
        get => _accessMode;
        set { SetProperty(ref _accessMode, value); HasChanges = true; }
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
            // Replaces WhenAnyValue canExecute observable — tell commands to re-check.
            SaveCommand.NotifyCanExecuteChanged();
            ResetCommand.NotifyCanExecuteChanged();
        }
    }

    public string ScriptContent
    {
        get => _scriptContent;
        set { SetProperty(ref _scriptContent, value); HasChanges = true; }
    }

    public bool IsTagLoaded => _currentTag != null;
    public bool IsScriptRequired => UpdateStrategy == UpdateStrategy.ScriptDriven;

    public List<TagDataType> AvailableDataTypes { get; } = Enum.GetValues<TagDataType>().ToList();
    public List<UpdateStrategy> AvailableUpdateStrategies { get; } = Enum.GetValues<UpdateStrategy>().ToList();
    public List<TagAccessMode> AvailableAccessModes { get; } = Enum.GetValues<TagAccessMode>().ToList();

    #endregion

    // Replaces ReactiveCommand<Unit, Unit> — RelayCommand accepts a plain Func<bool> for canExecute.
    public RelayCommand SaveCommand { get; }
    public RelayCommand ResetCommand { get; }

    public TagEditorViewModel()
    {
        SaveCommand = new RelayCommand(Save, () => IsTagLoaded && HasChanges);
        ResetCommand = new RelayCommand(Reset, () => IsTagLoaded);
    }

    public void LoadTag(Tag tag)
    {
        _currentTag = tag;
        _name = tag.Name;
        _dataType = tag.DataType;
        _updateStrategy = tag.UpdateStrategy;
        _updateInterval = tag.UpdateInterval;
        _initialValue = tag.InitialValue?.ToString() ?? string.Empty;
        _engineeringUnit = tag.EngineeringUnit ?? string.Empty;
        _accessMode = tag.AccessMode;
        _description = tag.Description ?? string.Empty;
        _scriptContent = tag.Metadata.TryGetValue("ScriptContent", out var script)
                               ? script : GetDefaultScript(tag.DataType);

        // Set backing fields directly (no setter) then raise INPC manually,
        // so none of the assignments above flip HasChanges = true.
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(DataType));
        OnPropertyChanged(nameof(UpdateStrategy));
        OnPropertyChanged(nameof(UpdateInterval));
        OnPropertyChanged(nameof(InitialValue));
        OnPropertyChanged(nameof(EngineeringUnit));
        OnPropertyChanged(nameof(AccessMode));
        OnPropertyChanged(nameof(Description));
        OnPropertyChanged(nameof(ScriptContent));
        OnPropertyChanged(nameof(IsTagLoaded));
        OnPropertyChanged(nameof(IsScriptRequired));

        HasChanges = false;
    }

    public void Clear()
    {
        _currentTag = null;
        _name = string.Empty;
        _dataType = TagDataType.Float;
        _updateStrategy = UpdateStrategy.Static;
        _updateInterval = 1000;
        _initialValue = string.Empty;
        _engineeringUnit = string.Empty;
        _accessMode = TagAccessMode.ReadWrite;
        _description = string.Empty;
        _scriptContent = string.Empty;

        OnPropertyChanged(nameof(IsTagLoaded));
        HasChanges = false;
    }

    private void Save()
    {
        if (_currentTag == null) return;

        var oldName = _currentTag.Name;
        if (oldName != Name)
        {
            bool isValid = true;
            ValidateNameChange?.Invoke(this, (_currentTag, oldName, Name, valid => isValid = valid));
            if (!isValid) { Name = oldName; return; }
        }

        _currentTag.Name = Name;
        _currentTag.DataType = DataType;
        _currentTag.UpdateStrategy = UpdateStrategy;
        _currentTag.UpdateInterval = UpdateInterval;
        _currentTag.InitialValue = ParseInitialValue();
        _currentTag.EngineeringUnit = string.IsNullOrWhiteSpace(EngineeringUnit) ? null : EngineeringUnit;
        _currentTag.AccessMode = AccessMode;
        _currentTag.Description = string.IsNullOrWhiteSpace(Description) ? null : Description;

        if (!string.IsNullOrWhiteSpace(ScriptContent))
            _currentTag.Metadata["ScriptContent"] = ScriptContent;
        else
            _currentTag.Metadata.Remove("ScriptContent");

        HasChanges = false;
        TagSaved?.Invoke(this, EventArgs.Empty);
    }

    private void Reset()
    {
        if (_currentTag != null) LoadTag(_currentTag);
    }

    private object? ParseInitialValue()
    {
        if (string.IsNullOrWhiteSpace(InitialValue)) return null;
        try
        {
            return DataType switch
            {
                TagDataType.Boolean => bool.Parse(InitialValue),
                TagDataType.Byte => byte.Parse(InitialValue),
                TagDataType.Int16 => short.Parse(InitialValue),
                TagDataType.Int32 => int.Parse(InitialValue),
                TagDataType.Int64 => long.Parse(InitialValue),
                TagDataType.Float => float.Parse(InitialValue),
                TagDataType.Double => double.Parse(InitialValue),
                TagDataType.String => InitialValue,
                TagDataType.DateTime => DateTime.Parse(InitialValue),
                _ => InitialValue
            };
        }
        catch { return InitialValue; }
    }

    private static string GetDefaultScript(TagDataType dataType) => dataType switch
    {
        TagDataType.Float or TagDataType.Double =>
@"// 'value' is the current tag value. Return the new value.
value = 50.0 + 25.0 * Math.Sin(DateTime.Now.TimeOfDay.TotalSeconds * 0.5);
return value;",
        TagDataType.Int32 or TagDataType.Int16 =>
@"// 'value' is the current tag value. Return the new value.
value = ((int)value + 1) % 100;
return value;",
        TagDataType.Boolean =>
@"// 'value' is the current tag value. Return the new value.
value = (DateTime.Now.Second % 10) < 5;
return value;",
        TagDataType.String =>
@"// 'value' is the current tag value. Return the new value.
value = DateTime.Now.ToString(""HH:mm:ss"");
return value;",
        _ =>
@"// 'value' is the current tag value. Return the new value.
value = /* your logic here */;
return value;"
    };
}