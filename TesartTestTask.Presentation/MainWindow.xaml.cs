using System.Windows;
using TesartTestTask.Presentation.ViewModels;

namespace TesartTestTask.Presentation;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel) 
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}