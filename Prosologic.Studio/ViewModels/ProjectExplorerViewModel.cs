using Prosologic.Core.Enums;
using Prosologic.Core.Models;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Prosologic.Studio.ViewModels
{
    public class ProjectExplorerViewModel : ViewModelBase
    {
        private TreeNodeViewModel? _selectedNode;
        public event EventHandler<Tag?>? TagSelected;

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
