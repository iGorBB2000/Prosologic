using Prosologic.Core.Models;
using ReactiveUI;

namespace Prosologic.Studio.ViewModels
{
    public class PropertiesPanelViewModel : ViewModelBase
    {
        private PropertyPanelType _currentPanel = PropertyPanelType.None;

        public ProjectPropertiesViewModel ProjectProperties { get; }
        public TagGroupEditorViewModel TagGroupEditor { get; }
        public TagEditorViewModel TagEditor { get; }

        public PropertyPanelType CurrentPanel
        {
            get => _currentPanel;
            private set
            {
                this.RaiseAndSetIfChanged(ref _currentPanel, value);
                this.RaisePropertyChanged(nameof(ShowProjectProperties));
                this.RaisePropertyChanged(nameof(ShowTagGroupEditor));
                this.RaisePropertyChanged(nameof(ShowTagEditor));
                this.RaisePropertyChanged(nameof(ShowNone));
            }
        }

        public bool ShowProjectProperties => CurrentPanel == PropertyPanelType.Project;
        public bool ShowTagGroupEditor => CurrentPanel == PropertyPanelType.TagGroup;
        public bool ShowTagEditor => CurrentPanel == PropertyPanelType.Tag;
        public bool ShowNone => CurrentPanel == PropertyPanelType.None;

        public PropertiesPanelViewModel()
        {
            ProjectProperties = new ProjectPropertiesViewModel();
            TagGroupEditor = new TagGroupEditorViewModel();
            TagEditor = new TagEditorViewModel();
        }

        public void ShowProject(Project project)
        {
            ProjectProperties.LoadProject(project);
            CurrentPanel = PropertyPanelType.Project;
        }

        public void ShowTagGroup(TagGroup group)
        {
            TagGroupEditor.LoadGroup(group);
            CurrentPanel = PropertyPanelType.TagGroup;
        }

        public void ShowTag(Tag tag)
        {
            TagEditor.LoadTag(tag);
            CurrentPanel = PropertyPanelType.Tag;
        }

        public void Clear()
        {
            ProjectProperties.Clear();
            TagGroupEditor.Clear();
            TagEditor.Clear();
            CurrentPanel = PropertyPanelType.None;
        }
    }

    public enum PropertyPanelType
    {
        None,
        Project,
        TagGroup,
        Tag
    }
}
