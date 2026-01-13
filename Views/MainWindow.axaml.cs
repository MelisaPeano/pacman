using Avalonia.Controls;

namespace PacmanAvalonia.Views;

public partial class MainWindow : Window

{
    public MainWindow()
    {
        InitializeComponent();
    }
    public string Greeting => "Hola Avalonia desde MVVM";

}
// UI estilos globales