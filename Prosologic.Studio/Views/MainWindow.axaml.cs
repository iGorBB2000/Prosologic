using Avalonia.Controls;
using Prosologic.Studio.ViewModels;

namespace Prosologic.Studio.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
}