using System.Windows;
using System.Windows.Shell;
using Prosologic.Studio.Services;
using Prosologic.Studio.ViewModels;

namespace Prosologic.Studio.Views;

public partial class MainWindow : Wpf.Ui.Controls.FluentWindow
{
    public MainWindow()
    {
        InitializeComponent();
        ContentRendered += OnContentRendered;

        var viewModel = new MainWindowViewModel();
        DataContext = viewModel;

        viewModel.Initialize(
            new FileDialogService(this),
            new MessageBoxService(this)
        );
    }

    private void OnContentRendered(object? sender, EventArgs e)
    {
        ContentRendered -= OnContentRendered;

        WindowChrome.SetWindowChrome(this, new WindowChrome
        {
            CaptionHeight = 48,
            ResizeBorderThickness = new Thickness(6),
            CornerRadius = new CornerRadius(0),
            GlassFrameThickness = new Thickness(0),
            UseAeroCaptionButtons = false,
        });
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        => WindowState = WindowState.Minimized;

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        => WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;

    private void CloseButton_Click(object sender, RoutedEventArgs e)
        => Close();
}