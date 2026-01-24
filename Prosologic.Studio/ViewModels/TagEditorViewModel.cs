using Prosologic.Core.Enums;
using Prosologic.Core.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;

namespace Prosologic.Studio.ViewModels
{
    public class TagEditorViewModel : ViewModelBase
    {
        private Tag? _currentTag;
        private bool _hasChanges;

        public event EventHandler? TagSaved;

        #region Properties
        private string _name = string.Empty;
        private TagDataType _dataType;
        private UpdateStrategy _updateStrategy;
        private int _updateInterval = 1000;
        private string _initialValue = string.Empty;
        private string _engineeringUnit = string.Empty;
        private TagAccessMode _accessMode;
        private string _description = string.Empty;

        public string Name
        {
            get => _name;
            set
            {
                this.RaiseAndSetIfChanged(ref _name, value);
                HasChanges = true;
            }
        }

        public TagDataType DataType
        {
            get => _dataType;
            set
            {
                this.RaiseAndSetIfChanged(ref _dataType, value);
                HasChanges = true;
            }
        }

        public UpdateStrategy UpdateStrategy
        {
            get => _updateStrategy;
            set
            {
                this.RaiseAndSetIfChanged(ref _updateStrategy, value);
                HasChanges = true;
                this.RaisePropertyChanged(nameof(IsScriptRequired));
            }
        }

        public int UpdateInterval
        {
            get => _updateInterval;
            set
            {
                this.RaiseAndSetIfChanged(ref _updateInterval, value);
                HasChanges = true;
            }
        }

        public string InitialValue
        {
            get => _initialValue;
            set
            {
                this.RaiseAndSetIfChanged(ref _initialValue, value);
                HasChanges = true;
            }
        }

        public string EngineeringUnit
        {
            get => _engineeringUnit;
            set
            {
                this.RaiseAndSetIfChanged(ref _engineeringUnit, value);
                HasChanges = true;
            }
        }

        public TagAccessMode AccessMode
        {
            get => _accessMode;
            set
            {
                this.RaiseAndSetIfChanged(ref _accessMode, value);
                HasChanges = true;
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                this.RaiseAndSetIfChanged(ref _description, value);
                HasChanges = true;
            }
        }

        public bool HasChanges
        {
            get => _hasChanges;
            private set => this.RaiseAndSetIfChanged(ref _hasChanges, value);
        }

        public bool IsTagLoaded => _currentTag != null;

        public bool IsScriptRequired => UpdateStrategy == UpdateStrategy.ScriptDriven;

        public List<TagDataType> AvailableDataTypes { get; } =
            Enum.GetValues<TagDataType>().ToList();

        public List<UpdateStrategy> AvailableUpdateStrategies { get; } =
            Enum.GetValues<UpdateStrategy>().ToList();

        public List<TagAccessMode> AvailableAccessModes { get; } =
            Enum.GetValues<TagAccessMode>().ToList();
        #endregion

        #region Commands
        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
        public ReactiveCommand<Unit, Unit> ResetCommand { get; }
        #endregion

        public TagEditorViewModel()
        {
            var canSave = this.WhenAnyValue(
                x => x.IsTagLoaded,
                x => x.HasChanges,
                (loaded, changed) => loaded && changed);

            SaveCommand = ReactiveCommand.Create(Save, canSave);
            ResetCommand = ReactiveCommand.Create(Reset, this.WhenAnyValue(x => x.IsTagLoaded));
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

            this.RaisePropertyChanged(nameof(Name));
            this.RaisePropertyChanged(nameof(DataType));
            this.RaisePropertyChanged(nameof(UpdateStrategy));
            this.RaisePropertyChanged(nameof(UpdateInterval));
            this.RaisePropertyChanged(nameof(InitialValue));
            this.RaisePropertyChanged(nameof(EngineeringUnit));
            this.RaisePropertyChanged(nameof(AccessMode));
            this.RaisePropertyChanged(nameof(Description));
            this.RaisePropertyChanged(nameof(IsTagLoaded));
            this.RaisePropertyChanged(nameof(IsScriptRequired));

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

            this.RaisePropertyChanged(nameof(IsTagLoaded));
            HasChanges = false;
        }

        private void Save()
        {
            if (_currentTag == null) return;

            _currentTag.Name = Name;
            _currentTag.DataType = DataType;
            _currentTag.UpdateStrategy = UpdateStrategy;
            _currentTag.UpdateInterval = UpdateInterval;
            _currentTag.InitialValue = ParseInitialValue();
            _currentTag.EngineeringUnit = string.IsNullOrWhiteSpace(EngineeringUnit) ? null : EngineeringUnit;
            _currentTag.AccessMode = AccessMode;
            _currentTag.Description = string.IsNullOrWhiteSpace(Description) ? null : Description;

            HasChanges = false;

            TagSaved?.Invoke(this, EventArgs.Empty);
        }

        private void Reset()
        {
            if (_currentTag == null) return;
            LoadTag(_currentTag);
        }

        private object? ParseInitialValue()
        {
            if (string.IsNullOrWhiteSpace(InitialValue))
                return null;

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
            catch
            {
                return InitialValue;
            }
        }
    }
}
