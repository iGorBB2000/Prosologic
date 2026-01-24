using Avalonia.Controls;
using Prosologic.Studio.ViewModels;

namespace Prosologic.Studio.Views;

public partial class TagEditorView : UserControl
{
    public TagEditorView()
    {
        InitializeComponent();
        DataContext = new TagEditorViewModel();
    }
}