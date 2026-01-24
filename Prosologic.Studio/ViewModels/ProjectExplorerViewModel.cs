using Prosologic.Core.Enums;
using Prosologic.Core.Models;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;

namespace Prosologic.Studio.ViewModels
{
    public class ProjectExplorerViewModel : ViewModelBase
    {
        private TreeNodeViewModel? _selectedNode;

        public ObservableCollection<TreeNodeViewModel> Nodes { get; } = new();

        public TreeNodeViewModel? SelectedNode
        {
            get => _selectedNode;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedNode, value);
                OnNodeSelected();
            }
        }

        #region Commands
        public ReactiveCommand<Unit, Unit> AddTagGroupCommand { get; }
        public ReactiveCommand<Unit, Unit> AddTagCommand { get; }
        public ReactiveCommand<Unit, Unit> DeleteCommand { get; }
        #endregion

        #region Events
        public event EventHandler<Tag?>? TagSelected;
        public event EventHandler<(string Name, TreeNodeViewModel Parent)>? AddTagGroupRequested;
        public event EventHandler<(string Name, TreeNodeViewModel Parent)>? AddTagRequested;
        public event EventHandler<TreeNodeViewModel>? DeleteRequested;
        #endregion

        public ProjectExplorerViewModel()
        {
            var canAddGroup = this.WhenAnyValue(x => x.SelectedNode)
                .Select(node => node?.NodeType == NodeType.Project ||
                               node?.NodeType == NodeType.TagGroup);
            AddTagGroupCommand = ReactiveCommand.Create(AddTagGroup, canAddGroup);

            var canAddTag = this.WhenAnyValue(x => x.SelectedNode)
                .Select(node => node?.NodeType == NodeType.TagGroup);
            AddTagCommand = ReactiveCommand.Create(AddTag, canAddTag);

            var canDelete = this.WhenAnyValue(x => x.SelectedNode)
                .Select(node => node?.NodeType == NodeType.Tag ||
                               node?.NodeType == NodeType.TagGroup);
            DeleteCommand = ReactiveCommand.Create(Delete, canDelete);
        }

        private void AddTagGroup()
        {
            if (SelectedNode == null) return;
            AddTagGroupRequested?.Invoke(this, ("", SelectedNode));
        }

        private void AddTag()
        {
            if (SelectedNode == null) return;
            AddTagRequested?.Invoke(this, ("", SelectedNode));
        }

        private void Delete()
        {
            if (SelectedNode == null) return;
            DeleteRequested?.Invoke(this, SelectedNode);
        }

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
            foreach (var rootNode in Nodes)
            {
                if (RemoveNodeRecursive(rootNode, node))
                {
                    SelectedNode = null;
                    return;
                }
            }
        }

        private bool RemoveNodeRecursive(TreeNodeViewModel parent, TreeNodeViewModel nodeToRemove)
        {
            if (parent.Children.Contains(nodeToRemove))
            {
                parent.Children.Remove(nodeToRemove);
                return true;
            }

            foreach (var child in parent.Children)
            {
                if (RemoveNodeRecursive(child, nodeToRemove))
                    return true;
            }

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
            {
                var groupNode = BuildTagGroupNode(group);
                projectNode.Children.Add(groupNode);
            }

            Nodes.Add(projectNode);
        }

        private TreeNodeViewModel BuildTagGroupNode(TagGroup group)
        {
            var node = new TreeNodeViewModel
            {
                Name = group.Name,
                NodeType = NodeType.TagGroup,
                DataContext = group,
                IsExpanded = true
            };

            foreach (var tag in group.Tags)
            {
                var tagNode = new TreeNodeViewModel
                {
                    Name = tag.Name,
                    NodeType = NodeType.Tag,
                    DataContext = tag
                };
                node.Children.Add(tagNode);
            }

            foreach (var subGroup in group.SubGroups)
            {
                var subNode = BuildTagGroupNode(subGroup);
                node.Children.Add(subNode);
            }

            return node;
        }

        private void OnNodeSelected()
        {
            if (SelectedNode?.NodeType == NodeType.Tag &&
                SelectedNode.DataContext is Tag tag)
            {
                TagSelected?.Invoke(this, tag);
            }
            else
            {
                TagSelected?.Invoke(this, null);
            }
        }

        public void Clear()
        {
            Nodes.Clear();
            SelectedNode = null;
        }
    }
}
