using System.Windows;
using SpeedEnforcement.Client.Core.ViewModels;

namespace SpeedEnforcement.Client.Wpf.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
