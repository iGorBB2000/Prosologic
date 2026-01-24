using Prosologic.Core.Enums;
using Prosologic.Core.Models;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace Prosologic.Studio.ViewModels
{
    public class TreeNodeViewModel : ViewModelBase
    {
        private string _name = string.Empty;
        private bool _isExpanded = true;
        private bool _isSelected;
        private object? _dataContext;

        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => this.RaiseAndSetIfChanged(ref _isSelected, value);
        }

        public object? DataContext
        {
            get => _dataContext;
            set
            {
                this.RaiseAndSetIfChanged(ref _dataContext, value);
                UpdateNameFromDataContext();
            }
        }

        public ObservableCollection<TreeNodeViewModel> Children { get; } = new();

        public NodeType NodeType { get; set; }

        private void UpdateNameFromDataContext()
        {
            if (_dataContext is Tag tag)
            {
                Name = tag.Name;
            }
            else if (_dataContext is TagGroup group)
            {
                Name = group.Name;
            }
            else if (_dataContext is Project project)
            {
                Name = project.ProjectName;
            }
        }

        public void RefreshName()
        {
            UpdateNameFromDataContext();
        }
    }
}
