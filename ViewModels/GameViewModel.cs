using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Threading; // Necesario para el Timer
using CommunityToolkit.Mvvm.ComponentModel;
using PacmanAvalonia.Models;
using PacmanAvalonia.Models.Entities;

namespace PacmanAvalonia.ViewModels;

public partial class GameViewModel : ViewModelBase
{
    // --- EVENTO PARA LA VISTA ---
    // Esto soluciona tu error: La vista se suscribe aquí para saber cuándo repintar
    public event Action? RequestRedraw;

    // --- PROPIEDADES ---
    public List<GameObject> GameObjects { get; private set; } = new();
    
    // "null!" le dice al compilador que lo inicializaremos en LoadMap
    public Pacman Player { get; private set; } = null!;

    [ObservableProperty]
    private int _score = 0;

    // --- VARIABLES DE CONTROL (Movimiento Continuo) ---
    private Direction _currentDirection = Direction.None; // Hacia donde va ahora
    private Direction _nextDirection = Direction.None;    // Hacia donde quiere ir el usuario
    
    private readonly MainWindowViewModel _mainViewModel;
    private readonly DispatcherTimer _gameTimer;

    // --- CONSTRUCTOR ---
    public GameViewModel(MainWindowViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
        
        InitializeGame();

        // CONFIGURACIÓN DEL GAME LOOP
        _gameTimer = new DispatcherTimer();
        _gameTimer.Interval = TimeSpan.FromMilliseconds(200); // Velocidad: 200ms por paso
        _gameTimer.Tick += GameLoop;
        _gameTimer.Start();
    }

    // --- CICLO DEL JUEGO (Se ejecuta cada 200ms) ---
    private void GameLoop(object? sender, EventArgs e)
    {
        bool moved = false;

        // 1. Intentar girar a la dirección que pidió el usuario (Buffer)
        if (_nextDirection != Direction.None && CanMove(_nextDirection))
        {
            _currentDirection = _nextDirection;
            _nextDirection = Direction.None; // Ya giramos, limpiamos la intención
        }

        // 2. Mover en la dirección actual
        if (_currentDirection != Direction.None && CanMove(_currentDirection))
        {
            MovePacman(_currentDirection);
            moved = true;
        }

        // 3. Si hubo movimiento, avisar a la vista que repinte
        if (moved)
        {
            RequestRedraw?.Invoke();
        }
    }

    // --- MÉTODOS DE CONTROL ---
    
    // Este método lo llama la Vista cuando presionas una tecla
    public void ChangeDirection(Direction newDir)
    {
        _nextDirection = newDir;
    }

    // Verifica si la siguiente casilla es válida (no es pared)
    private bool CanMove(Direction dir)
    {
        var (dx, dy) = GetDelta(dir);
        int newX = Player.X + dx;
        int newY = Player.Y + dy;

        // Verificar si hay una pared en esa posición
        return !GameObjects.Any(obj => obj is Wall && obj.X == newX && obj.Y == newY);
    }

    // Mueve al jugador y come monedas
    private void MovePacman(Direction dir)
    {
        var (dx, dy) = GetDelta(dir);
        Player.X += dx;
        Player.Y += dy;

        // Lógica de comer moneda
        var coin = GameObjects.OfType<Coin>().FirstOrDefault(c => c.X == Player.X && c.Y == Player.Y);
        if (coin != null)
        {
            GameObjects.Remove(coin);
            Score += 10;
        }
    }

    // Auxiliar para convertir Enum a coordenadas X,Y
    private (int x, int y) GetDelta(Direction dir)
    {
        return dir switch
        {
            Direction.Up => (0, -1),
            Direction.Down => (0, 1),
            Direction.Left => (-1, 0),
            Direction.Right => (1, 0),
            _ => (0, 0)
        };
    }
    
    private void InitializeGame()
    {
        // Mapa visual hecho con letras (mucho más fácil de editar)
        string[] mapLines = {
            "WWWWWWWWWWWWWWWWWWW",
            "W P      W        W", // P = Pacman
            "W WWWWWW W WWWWWW W",
            "W        W        W",
            "W WWWWWW W WWWWWW W",
            "W                 W",
            "WWWWWWWWWWWWWWWWWWW"
        };

        LoadMapFromString(mapLines);
    }

    private void LoadMapFromString(string[] lines)
    {
        GameObjects.Clear();
        
        for (int r = 0; r < lines.Length; r++)
        {
            for (int c = 0; c < lines[r].Length; c++)
            {
                char tile = lines[r][c];
                
                // IMPORTANTE: Asegúrate de que tus modelos (Wall, Coin, Pacman)
                // tengan el constructor que acepta (x, y).
                if (tile == 'W') GameObjects.Add(new Wall(c, r));
                else if (tile == ' ') GameObjects.Add(new Coin(c, r)); // Espacio = moneda
                else if (tile == 'P') 
                {
                    Player = new Pacman(c, r); // Guardamos referencia al jugador
                    GameObjects.Add(Player);
                }
            }
        }
    }
}