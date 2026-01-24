using Avalonia.Controls;
using Prosologic.Studio.Services;
using Prosologic.Studio.ViewModels;
using System;
using System.Diagnostics;

namespace Prosologic.Studio.Views;

public partial class MainWindow : Window
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