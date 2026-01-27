using Avalonia.Controls;
using Avalonia.Input;
using PacmanAvalonia.ViewModels;

namespace PacmanAvalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (DataContext is MainWindowViewModel viewModel)
        {
            // viewModel.MoverPacman(e.Key); // Esto lo implementaremos en el paso 3
        }
    }
}