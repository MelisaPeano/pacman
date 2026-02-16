using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Threading; 
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PacmanAvalonia.Models;
using PacmanAvalonia.Models.Entities;
using PacmanAvalonia.Models.enums;
using PacmanAvalonia.Services;

namespace PacmanAvalonia.ViewModels;

/// <summary>
/// Manages the core game logic, including the game loop, entity movement, collision detection, and level progression.
/// </summary>
public partial class GameViewModel : ViewModelBase
{
   /// <summary>
    /// Event triggered when the visual state of the game needs to be updated on the UI.
    /// </summary>
    public event Action? RequestRedraw;

    private readonly Random _random = new Random();
    private readonly AudioPlayer _audioPlayer;
    private readonly DispatcherTimer _powerModeTimer;
    private readonly DispatcherTimer _gameTimer;
    private readonly DispatcherTimer _cherryTimer;      
    private readonly DispatcherTimer _slowGhostsTimer;  
    private readonly MainWindowViewModel _mainViewModel;
    private readonly GameMode _currentMode;

    private int _mapWidth;
    private int _mapHeight;    
    private int _startPacmanX;
    private int _startPacmanY;
    private int _currentLevelIndex = 1;
    private bool _areGhostsSlowed = false;

    /// <summary>
    /// Gets the list of all entities currently active in the game world.
    /// </summary>
    public List<GameObject> GameObjects { get; private set; } = new();

    /// <summary>
    /// Gets the player-controlled Pacman entity.
    /// </summary>
    public Pacman Player { get; private set; } = null!;
    
    [ObservableProperty]
    private int _lives = 3; 

    [ObservableProperty]
    private bool _isGameOver = false;

    [ObservableProperty]
    private string _statusMessage = ""; 

    [ObservableProperty]
    private int _score = 0;

    [ObservableProperty]
    private Direction _currentDirection = Direction.None; 
    
    [ObservableProperty]
    private Direction _nextDirection = Direction.None;    
    
    [ObservableProperty]
    private bool _isMouthOpen = true;

    [ObservableProperty]
    private int _animationTick = 0;
    
    [ObservableProperty]
    private double _gridWidth;

    [ObservableProperty]
    private double _gridHeight;
    
    [ObservableProperty]
    private string _playerName = "PLAYER 1";
    
    [ObservableProperty]
    private bool _isScoreSaved = false;
    
    
    
    /// <summary>
    /// Initializes a new instance of the GameViewModel class with the specified game mode.
    /// </summary>
    /// <param name="mainViewModel">The main application view model for navigation.</param>
    /// <param name="mode">The selected game mode (Story or Survivor).</param>
    public GameViewModel(MainWindowViewModel mainViewModel, GameMode mode, int startingLevel = 1)
    {
        _mainViewModel = mainViewModel;
        _currentMode = mode;
        _audioPlayer = new AudioPlayer();
        
        if (_currentMode == GameMode.Story)
        {
            _currentLevelIndex = startingLevel; 
        }
        else
        {
            _currentLevelIndex = _random.Next(1, 11); 
        }

        InitializeGame();
        
        _audioPlayer.PlayIntro();
        
        _powerModeTimer = new DispatcherTimer();
        _powerModeTimer.Interval = TimeSpan.FromSeconds(10);
        _powerModeTimer.Tick += (s, e) => { DeactivatePowerMode(); };
        
        _gameTimer = new DispatcherTimer();
        _gameTimer.Interval = TimeSpan.FromMilliseconds(200); 
        _gameTimer.Tick += GameLoop;
        _gameTimer.Start();
        
        _cherryTimer = new DispatcherTimer();
        _cherryTimer.Interval = TimeSpan.FromSeconds(15);
        _cherryTimer.Tick += (s, e) => { SpawnCherry(); };
        _cherryTimer.Start();

        _slowGhostsTimer = new DispatcherTimer();
        _slowGhostsTimer.Interval = TimeSpan.FromSeconds(10);
        _slowGhostsTimer.Tick += (s, e) => 
        { 
            _areGhostsSlowed = false; 
            _slowGhostsTimer.Stop(); 
        };
    }

    /// <summary>
    /// Updates the player's desired movement direction based on user input.
    /// </summary>
    /// <param name="newDir">The new direction requested by the user.</param>
    public void ChangeDirection(Direction newDir)
    {
        _nextDirection = newDir;
    }

    private void GameLoop(object? sender, EventArgs e)
    {
        if (IsGameOver)
        {
            return;
        }

        if (_nextDirection != Direction.None && CanMove(_nextDirection))
        {
            CurrentDirection = _nextDirection; 
            _nextDirection = Direction.None;
        }

        if (CurrentDirection != Direction.None && CanMove(CurrentDirection))
        {
            MovePacman(CurrentDirection);
            CheckGhostCollisions();
        }

        MoveGhosts();
        CheckGhostCollisions();
        CheckWinCondition();

        _animationTick++;
        
        if (_animationTick % 2 == 0)
        {
            IsMouthOpen = !IsMouthOpen;
        }

        RequestRedraw?.Invoke();
    }
    [RelayCommand]
    public void SaveScore()
    {
        if (string.IsNullOrWhiteSpace(PlayerName) || IsScoreSaved) return;

        ScoreService.SaveScore(PlayerName, Score, _currentLevelIndex);
        IsScoreSaved = true; // Bloqueamos el botón
        StatusMessage = "SCORE SAVED!"; // Feedback visual
    }

    [RelayCommand]
    public void RetryLevel()
    {
        // Reseteamos banderas de Game Over
        IsGameOver = false;
        IsScoreSaved = false;
        StatusMessage = "";
        Lives = 3;
        Score = 0; // Opcional: ¿Quieres resetear el score al reintentar? Generalmente sí.
        
        // Recargamos el mapa actual
        InitializeGame();
    }

    [RelayCommand]
    public void ReturnToMenu()
    {
        _audioPlayer.StopAll();
        _gameTimer.Stop();
        _mainViewModel.NavigateTo(new MainMenuViewModel(_mainViewModel));
    }

    private void MoveGhosts()
    {

        int totalCicle = 80;

        double currentMoment = _animationTick % totalCicle;

        bool isTimeToRandomMode = currentMoment > 60;
        
        
        if (_areGhostsSlowed && _animationTick % 2 != 0)
        {
            return; 
        }
        
        if (!_areGhostsSlowed && !ShouldGhostsMoveByLevel())
        {
            return; 
        }

        foreach (var ghost in GameObjects.OfType<Ghost>())
        {
            if (ghost.X < 0)
            {
                continue;
            }
            if (ghost.State == GhostState.Vulnerable || isTimeToRandomMode && ghost.State == GhostState.Normal)
            {
                MoveGhostRandomly(ghost);
                continue;
            }

            var (targetX, targetY) = GetGhostTarget(ghost);
            MoveGhostTowards(ghost, targetX, targetY);
        }
        
    }

    private void MoveGhostRandomly(Ghost ghost)
    {
        var validMoves = new List<Direction>();
        foreach(Direction d in Enum.GetValues(typeof(Direction)))
        {
            if(d == Direction.None) 
            {
                continue;
            }

            var (nx, ny) = GetNextPositionWithWrap(ghost.X, ghost.Y, d);
            if(!IsWall(nx, ny)) 
            {
                validMoves.Add(d);
            }
        }
                
        if (validMoves.Count > 0)
        {
            var randomDir = validMoves[_random.Next(validMoves.Count)];
            var (fx, fy) = GetNextPositionWithWrap(ghost.X, ghost.Y, randomDir);
            ghost.X = fx; 
            ghost.Y = fy;
        }
        return;
    }

    private async void RespawnGhost(Ghost ghost)
    {
        ghost.X = -100;
        ghost.Y = -100;
    
        await System.Threading.Tasks.Task.Delay(3000);
    
        ghost.X = ghost.StartX;
        ghost.Y = ghost.StartY;
    
        ghost.LastDirX = 0;
        ghost.LastDirY = 0;
    
        ghost.State = GhostState.Normal;
    }
    
    /// <summary>
    /// Determina si los fantasmas deben moverse en este tick basándose en el nivel actual.
    /// Nivel 1: Lentos (50% vel). Nivel 5+: Máxima velocidad (100% vel).
    /// </summary>
    private bool ShouldGhostsMoveByLevel()
    {
        if (_currentLevelIndex >= 5) 
        {
            return true; 
        }
    
        int skipFactor = _currentLevelIndex + 1;
        
        if (_animationTick % skipFactor == 0)
        {
            return false; 
        }

        return true;
    }
    
    private (int x, int y) GetGhostTarget(Ghost ghost)
    {
        switch (ghost.Type)
        {
            case GhostType.Blinky: 
                return (Player.X, Player.Y);

            case GhostType.Pinky:
                var (pDx, pDy) = GetDelta(CurrentDirection); 
                return (Player.X + (pDx * 4), Player.Y + (pDy * 4));

            case GhostType.Inky:
                if (_random.NextDouble() > 0.5) 
                {
                    return (Player.X, Player.Y);
                }
                return (_random.Next(_mapWidth), _random.Next(_mapHeight));

            case GhostType.Clyde:
                double dist = Math.Sqrt(Math.Pow(Player.X - ghost.X, 2) + Math.Pow(Player.Y - ghost.Y, 2));
                if (dist < 8) 
                {
                    return (0, _mapHeight); 
                }
                return (Player.X, Player.Y); 
                
            default: 
                return (Player.X, Player.Y);
        }
    }

    private void MoveGhostTowards(Ghost ghost, int targetX, int targetY)
    {
        var directions = new[] { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
        
        double minDistance = double.MaxValue;
        Direction bestDir = Direction.None;
        int bestNextX = ghost.X;
        int bestNextY = ghost.Y;

        var shuffledDirs = directions.OrderBy(x => _random.Next()).ToList();

        foreach (var dir in shuffledDirs)
        {
            var (dx, dy) = GetDelta(dir);
            
            if (dx == -ghost.LastDirX && dy == -ghost.LastDirY && dx != 0 && dy != 0) 
            {
                continue; 
            }
            
            var (nx, ny) = GetNextPositionWithWrap(ghost.X, ghost.Y, dir);

            if (!IsWall(nx, ny))
            {
                double dist = Math.Pow(targetX - nx, 2) + Math.Pow(targetY - ny, 2);
                
                if (dist < minDistance)
                {
                    minDistance = dist;
                    bestDir = dir;
                    bestNextX = nx;
                    bestNextY = ny;
                }
            }
        }

        if (bestDir != Direction.None)
        {
            var (dx, dy) = GetDelta(bestDir);
            ghost.LastDirX = dx;
            ghost.LastDirY = dy;
            ghost.X = bestNextX;
            ghost.Y = bestNextY;
        }
    }

    private void UpdateGameSpeed()
    {
        int coinsLeft = GameObjects.OfType<Coin>().Count();
        if (coinsLeft < 10) 
        {
            _gameTimer.Interval = TimeSpan.FromMilliseconds(130); 
        }
        else if (coinsLeft < 50) 
        {
            _gameTimer.Interval = TimeSpan.FromMilliseconds(170); 
        }
    }

    private void CheckGhostCollisions()
    {
        if (Player is null) 
        {
            return;
        }

        var collidedGhost = GameObjects
            .OfType<Ghost>()
            .Where(ghost => ghost.X == Player.X && ghost.Y == Player.Y)
            .ToList();

        foreach (var ghost in collidedGhost)
        {
            if (IsGameOver) 
            {
                return; 
            }

            if (ghost.State == GhostState.Vulnerable)
            {
                Score += 200;
                _audioPlayer.PlayEatGhost(); 
                
               RespawnGhost(ghost);
            }
            else
            {
                _audioPlayer.PlayDeath();
                Lives--;

                if (Lives > 0)
                {
                    ResetPositions();
                    return; 
                }
                else
                {
                    GameOver("GAME OVER");
                    return;
                }
            }
        }
    }

    private bool IsWall(int x, int y)
    {
        return GameObjects.OfType<Wall>().Any(w => w.X == x && w.Y == y);
    }

    private bool CanMove(Direction dir)
    {
        var (nextX, nextY) = GetNextPositionWithWrap(Player.X, Player.Y, dir);
        return !IsWall(nextX, nextY);
    }

    private void MovePacman(Direction dir)
    {
        var (nextX, nextY) = GetNextPositionWithWrap(Player.X, Player.Y, dir);
        
        var cherry = GameObjects.OfType<Cherry>().FirstOrDefault(c => c.X == Player.X && c.Y == Player.Y);
        
        if (cherry is not null)
        {
            GameObjects.Remove(cherry);
            _audioPlayer.PlayWaka(); 

            if (cherry.Type == CherryType.Blue)
            {
                Lives++;
            }
            else
            {
                _areGhostsSlowed = true;
                _slowGhostsTimer.Stop();
                _slowGhostsTimer.Start();
            }
        }

        if (!IsWall(nextX, nextY))
        {
            Player.X = nextX;
            Player.Y = nextY;

            var coin = GameObjects.OfType<Coin>().FirstOrDefault(c => c.X == Player.X && c.Y == Player.Y);
        
            if (coin is not null)
            {
                GameObjects.Remove(coin);
                Score += 10;
                _audioPlayer.PlayWaka();
                UpdateGameSpeed();

                if (coin.IsPowerPellet)
                {
                    ActivatePowerMode();
                    _audioPlayer.PlayPowerPellet();
                    Score += 40; 
                }
                else
                {
                    _audioPlayer.PlayWaka();
                }
            }
        }
    }

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

    private void CheckWinCondition()
    {
        if (!GameObjects.OfType<Coin>().Any())
        {
            _audioPlayer.StopAll();
            
            if (_currentMode == GameMode.Story)
            {
                _currentLevelIndex++;

                if (_currentLevelIndex > 10)
                {
                    _audioPlayer.PlayWin();
                    GameOver("¡CAMPAÑA COMPLETADA!");
                }
                else
                {
                    InitializeGame(); 
                }
            }
            else 
            {
                double newInterval = Math.Max(50, _gameTimer.Interval.TotalMilliseconds - 10);
                _gameTimer.Interval = TimeSpan.FromMilliseconds(newInterval);
                
                _currentLevelIndex = _random.Next(1, 11);
                
                InitializeGame();
            }
        }
    }

    private void ResetPositions()
    {
        Player.X = _startPacmanX;
        Player.Y = _startPacmanY;
        
        CurrentDirection = Direction.None;
        _nextDirection = Direction.None;
        IsMouthOpen = true; 
        
        RequestRedraw?.Invoke();
    }

    private void GameOver(string message)
    {
        _gameTimer.Stop();
        IsGameOver = true;
        StatusMessage = message;
        _audioPlayer.StopAll(); 
    }

    private void InitializeGame()
    {
        _gameTimer?.Stop();

        string fileName = $"Level_layout{_currentLevelIndex}.txt";
        string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "maps", fileName);
        
        if (System.IO.File.Exists(path))
        {
            string[] lines = System.IO.File.ReadAllLines(path);
            LoadMapFromString(lines);
        }
        else
        {
            string[] emergencyMap = {
                "WWWWWWWWWW",
                "W P    o W", 
                "WWWWWWWWWW"
            };
            LoadMapFromString(emergencyMap);
        }

        CurrentDirection = Direction.None;
        _nextDirection = Direction.None;
        IsMouthOpen = true;
        _areGhostsSlowed = false; 
        
        RequestRedraw?.Invoke();
        _gameTimer?.Start();
    }

    private void LoadMapFromString(string[] lines)
    {
        try
        {
            GameObjects.Clear();
            Player = null!; 
            
            _mapHeight = lines.Length;
            _mapWidth = lines[0].Length;
            
            GridWidth = _mapWidth * 30;
            GridHeight = _mapHeight * 30;

            for (int r = 0; r < lines.Length; r++)
            {
                for (int c = 0; c < lines[r].Length; c++)
                {
                    char tile = lines[r][c];

                    if (IsWallChar(tile))
                    {
                        GameObjects.Add(new Wall(c, r, tile));
                    }
                    else if (tile == 'o' || tile == '.')
                    {
                        GameObjects.Add(new Coin(c, r, false)); 
                    }
                    else if (tile == 'O') 
                    {
                        GameObjects.Add(new Coin(c, r, true)); 
                    }                    
                    else if (tile == 'P' || tile == 'C')
                    {
                        if (Player is null)
                        {
                            Player = new Pacman(c, r);
                            _startPacmanX = c;
                            _startPacmanY = r;
                            GameObjects.Add(Player);
                        }
                    }
                    else if (char.IsDigit(tile))
                    {
                        GhostType type = tile switch
                        {
                            '1' => GhostType.Blinky,
                            '2' => GhostType.Pinky,
                            '3' => GhostType.Inky,
                            '4' => GhostType.Clyde,
                            _ => GhostType.Blinky
                        };
        
                        GameObjects.Add(new Ghost(c, r, type));
                    }
                    else if (tile == '=')
                    {
                        GameObjects.Add(new GhostDoor(c, r));
                    }
                }
            }

            if (Player is null)
            {
                Player = new Pacman(1, 1);
                GameObjects.Add(Player);
            }
        }
        catch 
        {
        }
    }

    private bool IsWallChar(char c)
    {
        return "-|QWASLRDU=".Contains(c);
    }
    
    private (int x, int y) GetNextPositionWithWrap(int currentX, int currentY, Direction dir)
    {
        var (dx, dy) = GetDelta(dir);
        int nextX = currentX + dx;
        int nextY = currentY + dy;

        if (nextX < 0)
        {
            nextX = _mapWidth - 1;
        }
        else if (nextX >= _mapWidth)
        {
            nextX = 0;
        }

        if (nextY < 0)
        {
            nextY = _mapHeight - 1;
        }
        else if (nextY >= _mapHeight)
        {
            nextY = 0;
        }

        return (nextX, nextY);
    }

    private void ActivatePowerMode()
    {
        try
        {
            _powerModeTimer.Stop();
            _powerModeTimer.Start();

            foreach (var ghost in GameObjects.OfType<Ghost>())
            {
                ghost.State = GhostState.Vulnerable;
            }
        }
        catch 
        {
        }
    }

    private void DeactivatePowerMode()
    {
        _powerModeTimer.Stop();
        foreach (var ghost in GameObjects.OfType<Ghost>())
        {
            ghost.State = GhostState.Normal;
        }
    }
    
    private void SpawnCherry()
    {
        if (GameObjects.OfType<Cherry>().Any()) 
        {
            return;
        }

        int maxAttempts = 50;
        for (int i = 0; i < maxAttempts; i++)
        {
            int rx = _random.Next(_mapWidth);
            int ry = _random.Next(_mapHeight);

            if (!IsWall(rx, ry) && !GameObjects.OfType<Ghost>().Any(g => g.X == rx && g.Y == ry))
            {
                CherryType type = _random.Next(0, 2) == 0 ? CherryType.Red : CherryType.Blue;
                GameObjects.Add(new Cherry(rx, ry, type));
                break;
            }
        }
    }
}