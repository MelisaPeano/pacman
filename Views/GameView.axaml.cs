using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using PacmanAvalonia.ViewModels;
using PacmanAvalonia.Models;
using System.Collections.Generic;
using PacmanAvalonia.Models.Entities;
using PacmanAvalonia.Services;
namespace PacmanAvalonia.Views;

public partial class GameView : UserControl
{
   private GameRenderer? _renderer; // Referencia al renderer

    public GameView()
    {
        InitializeComponent();
        this.Loaded += OnViewLoaded;
        this.Unloaded += OnViewUnloaded;
    }
    
    private void OnViewLoaded(object? sender, RoutedEventArgs e)
    {
        // Inicializamos el Renderer pasándole el Canvas del XAML
        var canvas = this.FindControl<Canvas>("GameCanvas");
        if (canvas != null)
        {
            _renderer = new GameRenderer(canvas);
        }

        if (DataContext is GameViewModel vm && _renderer != null)
        {
            // Dibujo inicial
            _renderer.Draw(vm.GameObjects);
            
            // Suscripción usando lambda simple
            vm.RequestRedraw += () => _renderer.Draw(vm.GameObjects);
        }
        
        this.Focus();
    }

    private void OnViewUnloaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is GameViewModel vm)
        {
            // CORRECCIÓN: Nos desuscribimos limpiamente
            vm.RequestRedraw -= HandleRedraw;
        }
    }

    // Método auxiliar para manejar el redibujado
    private void HandleRedraw()
    {
        if (DataContext is GameViewModel vm && _renderer != null)
        {
            _renderer.Draw(vm.GameObjects);
        }
    }
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (DataContext is GameViewModel vm)
        {
            // Tu lógica de input se mantiene igual, ¡estaba perfecta!
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