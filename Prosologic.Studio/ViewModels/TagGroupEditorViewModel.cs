using Prosologic.Core.Models;
using ReactiveUI;
using System;
using System.Reactive;

namespace Prosologic.Studio.ViewModels
{
    public class TagGroupEditorViewModel : ViewModelBase
    {
        private TagGroup? _currentGroup;
        private bool _hasChanges;

        private string _name = string.Empty;
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

        public bool IsGroupLoaded => _currentGroup != null;

        #region Commands
        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
        public ReactiveCommand<Unit, Unit> ResetCommand { get; }
        #endregion

        public event EventHandler<(TagGroup Group, string OldName, string NewName)>? GroupNameChanged;
        public event EventHandler? GroupSaved;

        public TagGroupEditorViewModel()
        {
            var canSave = this.WhenAnyValue(
                x => x.IsGroupLoaded,
                x => x.HasChanges,
                (loaded, changed) => loaded && changed);

            SaveCommand = ReactiveCommand.Create(Save, canSave);
            ResetCommand = ReactiveCommand.Create(Reset, this.WhenAnyValue(x => x.IsGroupLoaded));
        }

        public void LoadGroup(TagGroup group)
        {
            _currentGroup = group;

            _name = group.Name;
            _description = group.Description ?? string.Empty;

            this.RaisePropertyChanged(nameof(Name));
            this.RaisePropertyChanged(nameof(Description));
            this.RaisePropertyChanged(nameof(IsGroupLoaded));

            HasChanges = false;
        }

        public void Clear()
        {
            _currentGroup = null;
            _name = string.Empty;
            _description = string.Empty;

            this.RaisePropertyChanged(nameof(IsGroupLoaded));
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
            {
                GroupNameChanged?.Invoke(this, (_currentGroup, oldName, Name));
            }

            GroupSaved?.Invoke(this, EventArgs.Empty);
        }

        private void Reset()
        {
            if (_currentGroup == null) return;
            LoadGroup(_currentGroup);
        }
    }
}
