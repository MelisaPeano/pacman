using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using PacmanAvalonia.ViewModels;
using PacmanAvalonia.Models.enums;
using PacmanAvalonia.Services;
namespace PacmanAvalonia.Views;

public partial class GameView : UserControl
{
    private GameRenderer? _renderer; 

    public GameView()
    {
        InitializeComponent();
        this.Loaded += OnViewLoaded;
        this.Unloaded += OnViewUnloaded;
    }
    
    private void OnViewLoaded(object? sender, RoutedEventArgs e)
    {
        var canvas = this.FindControl<Canvas>("GameCanvas");
        if (canvas is not null) // Sintaxis corregida
        {
            _renderer = new GameRenderer(canvas);
        }

        // VERIFICACIÓN: Renderer y ViewModel no deben ser nulos
        if (DataContext is GameViewModel vm && _renderer is not null)
        {
            // CORRECCIÓN CLAVE: Pasamos 'vm' completo, no la lista
            _renderer.Draw(vm);
            
            // Suscripción
            vm.RequestRedraw += HandleRedraw;
        }
        
        this.Focus();
    }

    private void OnViewUnloaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is GameViewModel vm)
        {
            vm.RequestRedraw -= HandleRedraw;
        }
    }

    private void HandleRedraw()
    {
        if (DataContext is GameViewModel vm && _renderer is not null)
        {
            // CORRECCIÓN CLAVE: Pasamos 'vm' completo
            _renderer.Draw(vm);
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (DataContext is GameViewModel vm)
        {
            switch (e.Key)
            {
                case Key.Up:    vm.ChangeDirection(Direction.Up); break;
                case Key.Down:  vm.ChangeDirection(Direction.Down); break;
                case Key.Left:  vm.ChangeDirection(Direction.Left); break;
                case Key.Right: vm.ChangeDirection(Direction.Right); break;
            }
        }
    }
}