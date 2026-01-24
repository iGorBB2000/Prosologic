using Prosologic.Core.Enums;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace Prosologic.Studio.ViewModels
{
    public class TreeNodeViewModel : ViewModelBase
    {
        private string _name = string.Empty;
        private bool _isExpanded = true;
        private bool _isSelected;

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

        public ObservableCollection<TreeNodeViewModel> Children { get; } = new();

        public NodeType NodeType { get; set; }

        public object? DataContext { get; set; }
    }
}
