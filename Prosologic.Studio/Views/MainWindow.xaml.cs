using System.Windows;
using System.Windows.Shell;
using Prosologic.Studio.Services;
using Prosologic.Studio.ViewModels;

namespace Prosologic.Studio.Views;

public partial class MainWindow : Wpf.Ui.Controls.FluentWindow
{
    private double _savedContainerHeight = 160;

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

        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(MainWindowViewModel.ShowContainersPanel))
                ApplyContainerPanelVisibility(viewModel.ShowContainersPanel);
        };

        // Apply initial state (hidden by default)
        ApplyContainerPanelVisibility(viewModel.ShowContainersPanel);
    }

    private void ApplyContainerPanelVisibility(bool show)
    {
        if (show)
        {
            ContainerRow.MinHeight = 80;
            ContainerRow.Height = new GridLength(_savedContainerHeight, GridUnitType.Pixel);
            SplitterRow.Height = new GridLength(4, GridUnitType.Pixel);
            ContainerSplitter.Visibility = Visibility.Visible;
            ContainerBorder.Visibility = Visibility.Visible;
        }
        else
        {
            if (ContainerRow.ActualHeight > 0)
                _savedContainerHeight = ContainerRow.ActualHeight;

            ContainerBorder.Visibility = Visibility.Collapsed;
            ContainerSplitter.Visibility = Visibility.Collapsed;
            ContainerRow.MinHeight = 0;
            ContainerRow.Height = new GridLength(0, GridUnitType.Pixel);
            SplitterRow.Height = new GridLength(0, GridUnitType.Pixel);
        }
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