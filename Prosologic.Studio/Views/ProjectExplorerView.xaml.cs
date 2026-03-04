using Prosologic.Studio.ViewModels;
using System.Windows.Controls;

namespace Prosologic.Studio.Views
{
    /// <summary>
    /// Interaction logic for ProjectExplorerView.xaml
    /// </summary>
    public partial class ProjectExplorerView : UserControl
    {
        public ProjectExplorerView()
        {
            InitializeComponent();
            DataContext = new ProjectExplorerViewModel();
        }

        private void TreeView_SelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is ProjectExplorerViewModel vm)
                vm.SelectedNode = e.NewValue as TreeNodeViewModel;
        }
    }
}
