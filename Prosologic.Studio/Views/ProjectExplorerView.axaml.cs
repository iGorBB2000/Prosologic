using Avalonia.Controls;
using Prosologic.Studio.ViewModels;

namespace Prosologic.Studio.Views;

public partial class ProjectExplorerView : UserControl
{
    public ProjectExplorerView()
    {
        InitializeComponent();
        DataContext = new ProjectExplorerViewModel();
    }
}