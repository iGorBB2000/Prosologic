using Prosologic.Studio.Services;
using Prosologic.Studio.ViewModels;

namespace Prosologic.Studio.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Wpf.Ui.Controls.FluentWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            var viewModel = new MainWindowViewModel();
            DataContext = viewModel;

            viewModel.Initialize(
                new FileDialogService(this),
                new MessageBoxService(this)
            );

        }
    }
}