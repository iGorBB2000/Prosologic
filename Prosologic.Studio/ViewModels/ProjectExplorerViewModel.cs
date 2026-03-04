using CommunityToolkit.Mvvm.Input;
using Prosologic.Core.Enums;
using Prosologic.Core.Models;
using System.Collections.ObjectModel;

namespace Prosologic.Studio.ViewModels;

public class ProjectExplorerViewModel : ViewModelBase
{
    private TreeNodeViewModel? _selectedNode;

    public ObservableCollection<TreeNodeViewModel> Nodes { get; } = new();

    public TreeNodeViewModel? SelectedNode
    {
        get => _selectedNode;
        set
        {
            SetProperty(ref _selectedNode, value);
            AddTagGroupCommand.NotifyCanExecuteChanged();
            AddTagCommand.NotifyCanExecuteChanged();
            DeleteCommand.NotifyCanExecuteChanged();
            OnNodeSelected();
        }
    }

    public RelayCommand AddTagGroupCommand { get; }
    public RelayCommand AddTagCommand { get; }
    public RelayCommand DeleteCommand { get; }

    public event EventHandler<(string Name, TreeNodeViewModel Parent)>? AddTagGroupRequested;
    public event EventHandler<(string Name, TreeNodeViewModel Parent)>? AddTagRequested;
    public event EventHandler<TreeNodeViewModel>? DeleteRequested;
    public event EventHandler<(NodeType Type, object? Data)>? SelectionChanged;

    public ProjectExplorerViewModel()
    {
        AddTagGroupCommand = new RelayCommand(AddTagGroup, () =>
            SelectedNode?.NodeType is NodeType.Project or NodeType.TagGroup);

        AddTagCommand = new RelayCommand(AddTag, () =>
            SelectedNode?.NodeType == NodeType.TagGroup);

        DeleteCommand = new RelayCommand(Delete, () =>
            SelectedNode?.NodeType is NodeType.Tag or NodeType.TagGroup);
    }

    private void AddTagGroup() =>
        AddTagGroupRequested?.Invoke(this, (string.Empty, SelectedNode!));

    private void AddTag() =>
        AddTagRequested?.Invoke(this, (string.Empty, SelectedNode!));

    private void Delete() =>
        DeleteRequested?.Invoke(this, SelectedNode!);

    public void AddTagGroupNode(TagGroup group, TreeNodeViewModel parentNode)
    {
        var node = new TreeNodeViewModel
        {
            Name = group.Name,
            NodeType = NodeType.TagGroup,
            DataContext = group,
            IsExpanded = true
        };
        parentNode.Children.Add(node);
        parentNode.IsExpanded = true;
        SelectedNode = node;
    }

    public void AddTagNode(Tag tag, TreeNodeViewModel parentNode)
    {
        var node = new TreeNodeViewModel
        {
            Name = tag.Name,
            NodeType = NodeType.Tag,
            DataContext = tag
        };
        parentNode.Children.Add(node);
        parentNode.IsExpanded = true;
        SelectedNode = node;
    }

    public void RemoveNode(TreeNodeViewModel node)
    {
        foreach (var root in Nodes)
        {
            if (RemoveNodeRecursive(root, node))
            {
                SelectedNode = null;
                return;
            }
        }
    }

    private static bool RemoveNodeRecursive(TreeNodeViewModel parent, TreeNodeViewModel target)
    {
        if (parent.Children.Remove(target)) return true;
        foreach (var child in parent.Children)
            if (RemoveNodeRecursive(child, target)) return true;
        return false;
    }

    public void LoadProject(Project project)
    {
        Nodes.Clear();

        var projectNode = new TreeNodeViewModel
        {
            Name = project.ProjectName,
            NodeType = NodeType.Project,
            DataContext = project,
            IsExpanded = true
        };

        foreach (var group in project.TagGroups)
            projectNode.Children.Add(BuildTagGroupNode(group));

        Nodes.Add(projectNode);
    }

    private static TreeNodeViewModel BuildTagGroupNode(TagGroup group)
    {
        var node = new TreeNodeViewModel
        {
            Name = group.Name,
            NodeType = NodeType.TagGroup,
            DataContext = group,
            IsExpanded = true
        };

        foreach (var tag in group.Tags)
            node.Children.Add(new TreeNodeViewModel
            {
                Name = tag.Name,
                NodeType = NodeType.Tag,
                DataContext = tag
            });

        foreach (var sub in group.SubGroups)
            node.Children.Add(BuildTagGroupNode(sub));

        return node;
    }

    private void OnNodeSelected()
    {
        SelectionChanged?.Invoke(this, SelectedNode != null
            ? (SelectedNode.NodeType, SelectedNode.DataContext)
            : (NodeType.Project, null));
    }

    public void Clear()
    {
        Nodes.Clear();
        SelectedNode = null;
    }
}