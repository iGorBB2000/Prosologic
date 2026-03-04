using Prosologic.Core.Enums;
using Prosologic.Core.Models;
using System.Collections.ObjectModel;

namespace Prosologic.Studio.ViewModels;

public class TreeNodeViewModel : ViewModelBase
{
    private string _name = string.Empty;
    private bool _isExpanded = true;
    private bool _isSelected;
    private object? _dataContext;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public object? DataContext
    {
        get => _dataContext;
        set
        {
            SetProperty(ref _dataContext, value);
            UpdateNameFromDataContext();
        }
    }

    public ObservableCollection<TreeNodeViewModel> Children { get; } = new();

    public NodeType NodeType { get; set; }

    private void UpdateNameFromDataContext()
    {
        Name = _dataContext switch
        {
            Tag tag => tag.Name,
            TagGroup group => group.Name,
            Project project => project.ProjectName,
            _ => _name
        };
    }

    public void RefreshName() => UpdateNameFromDataContext();
}