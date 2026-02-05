using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Threading; 
using CommunityToolkit.Mvvm.ComponentModel;
using PacmanAvalonia.Models;
using PacmanAvalonia.Models.Entities;
using PacmanAvalonia.Models.enums;
using PacmanAvalonia.Services;

namespace PacmanAvalonia.ViewModels;

public partial class GameViewModel : ViewModelBase
{
    public event Action? RequestRedraw;
    private readonly Random _random = new Random();
    private readonly AudioPlayer _audioPlayer;
    
    private readonly DispatcherTimer _powerModeTimer;
    
    private int _mapWidth;
    private int _mapHeight;    public List<GameObject> GameObjects { get; private set; } = new();
    public Pacman Player { get; private set; } = null!;
    
    [ObservableProperty]
    private int _lives = 3; // Empezamos con 3 vidas

    [ObservableProperty]
    private bool _isGameOver = false;

    [ObservableProperty]
    private string _statusMessage = ""; // Para mostrar "GAME OVER" o "YOU WIN"

    // Guardamos la posición inicial para "revivir" a Pacman
    private int _startPacmanX;
    private int _startPacmanY;

    [ObservableProperty]
    private int _score = 0;

    [ObservableProperty]
    private Direction _currentDirection = Direction.None; 
    
    [ObservableProperty]// Hacia donde va ahora
    private Direction _nextDirection = Direction.None;    // Hacia donde quiere ir el usuario
    
    private readonly MainWindowViewModel _mainViewModel;
    private readonly DispatcherTimer _gameTimer;
    
    [ObservableProperty]
    private bool _isMouthOpen = true;
    [ObservableProperty]
    private int _animationTick = 0;
    
    private readonly DispatcherTimer _cherryTimer;      // Controla aparición
    private readonly DispatcherTimer _slowGhostsTimer;  // Controla duración del efecto lento
    private bool _areGhostsSlowed = false;

    // --- CONSTRUCTOR ---
    public GameViewModel(MainWindowViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
        _audioPlayer = new AudioPlayer();
        
        InitializeGame();
        
        _audioPlayer.PlayIntro();
        
        _powerModeTimer = new DispatcherTimer();
        _powerModeTimer.Interval = TimeSpan.FromSeconds(10);
        _powerModeTimer.Tick += (s, e) => { DeactivatePowerMode(); };
        
        // CONFIGURACIÓN DEL GAME LOOP
        _gameTimer = new DispatcherTimer();
        _gameTimer.Interval = TimeSpan.FromMilliseconds(200); // Velocidad: 200ms por paso
        _gameTimer.Tick += GameLoop;
        _gameTimer.Start();
        
        _cherryTimer = new DispatcherTimer();
        _cherryTimer.Interval = TimeSpan.FromSeconds(15);
        _cherryTimer.Tick += (s, e) => SpawnCherry();
        _cherryTimer.Start();

        // CONFIGURAR TIMER DE EFECTO LENTO (Dura 10 segundos)
        _slowGhostsTimer = new DispatcherTimer();
        _slowGhostsTimer.Interval = TimeSpan.FromSeconds(10);
        _slowGhostsTimer.Tick += (s, e) => 
        { 
            _areGhostsSlowed = false; 
            _slowGhostsTimer.Stop(); 
        };
    }

    private void GameLoop(object? sender, EventArgs e)
    {
        if (IsGameOver)
        {
            return;
        }

        bool moved = false;

        // ... (Tu lógica de movimiento de Pacman y buffer aquí) ...
        // IMPORTANTE: Asegúrate de usar la propiedad 'CurrentDirection' (Mayúscula)
        // en lugar de '_currentDirection' en tus if/else.
        
        // Ejemplo de actualización:
        if (_nextDirection != Direction.None && CanMove(_nextDirection))
        {
            CurrentDirection = _nextDirection; // Asignamos a la pública
            _nextDirection = Direction.None;
        }

        if (CurrentDirection != Direction.None && CanMove(CurrentDirection))
        {
            MovePacman(CurrentDirection);
            moved = true;
        }

        // ... (Tu lógica de MoveGhosts, Colisiones, etc.) ...
        MoveGhosts();
        CheckGhostCollisions();
        CheckWinCondition();

        // --- NUEVA LÓGICA DE ANIMACIÓN ---
        _animationTick++;
        
        // Cada 2 ticks (aprox 400ms) alternamos la boca
        if (_animationTick % 2 == 0)
        {
            IsMouthOpen = !IsMouthOpen;
        }

        // 3. Redibujar siempre para ver la animación suave
        RequestRedraw?.Invoke();
    }
    
    private void MoveGhosts()
    {
        foreach (var ghost in GameObjects.OfType<Ghost>())
        {
            // Si está asustado, mantenemos tu lógica antigua (aleatorio) para no romper nada
            if (ghost.State == GhostState.Vulnerable)
            {
                // Movimiento aleatorio simple
                var validMoves = new List<Direction>();
                foreach(Direction d in Enum.GetValues(typeof(Direction)))
                {
                    if(d == Direction.None) continue;
                    var (nx, ny) = GetNextPositionWithWrap(ghost.X, ghost.Y, d);
                    if(!IsWall(nx, ny)) validMoves.Add(d);
                }
                
                if (validMoves.Count > 0)
                {
                    var randomDir = validMoves[_random.Next(validMoves.Count)];
                    var (fx, fy) = GetNextPositionWithWrap(ghost.X, ghost.Y, randomDir);
                    ghost.X = fx; 
                    ghost.Y = fy;
                }
                continue; // Salta al siguiente fantasma
            }

            // --- LÓGICA INTELIGENTE (MODO NORMAL) ---
            var (targetX, targetY) = GetGhostTarget(ghost);
            MoveGhostTowards(ghost, targetX, targetY);
        }
    }
    
    private (int x, int y) GetGhostTarget(Ghost ghost)
    {
        switch (ghost.Type)
        {
            case GhostType.Blinky: 
                // EL CAZADOR: Va directo a donde está Pacman
                return (Player.X, Player.Y);

            case GhostType.Pinky:
                // EL EMBOSCADOR: Apunta 4 casillas ADELANTE de Pacman
                var (pDx, pDy) = GetDelta(CurrentDirection); 
                return (Player.X + (pDx * 4), Player.Y + (pDy * 4));

            case GhostType.Inky:
                // EL CAPRICHOSO (blueGhost): Lo hacemos impredecible
                // 50% persigue a Pacman, 50% va a una esquina random
                if (_random.NextDouble() > 0.5) return (Player.X, Player.Y);
                return (_random.Next(_mapWidth), _random.Next(_mapHeight));

            case GhostType.Clyde:
                // EL MIEDOSO: Persigue si está lejos (>8 pasos), pero huye si se acerca
                double dist = Math.Sqrt(Math.Pow(Player.X - ghost.X, 2) + Math.Pow(Player.Y - ghost.Y, 2));
                if (dist < 8) return (0, _mapHeight); // Huye a la esquina inferior
                return (Player.X, Player.Y); // Persigue
                
            default: return (Player.X, Player.Y);
        }
    }

    // 4. PATHFINDING (GPS)
    private void MoveGhostTowards(Ghost ghost, int targetX, int targetY)
    {
        var directions = new[] { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
        
        double minDistance = double.MaxValue;
        Direction bestDir = Direction.None;
        int bestNextX = ghost.X;
        int bestNextY = ghost.Y;

        // Orden aleatorio para romper empates y que no se vean robóticos
        var shuffledDirs = directions.OrderBy(x => _random.Next()).ToList();

        foreach (var dir in shuffledDirs)
        {
            var (dx, dy) = GetDelta(dir);
            
            // REGLA: No vale girar 180 grados (volver por donde vino)
            // Si intenta ir al opuesto de su última dirección, lo saltamos
            if (dx == -ghost.LastDirX && dy == -ghost.LastDirY && dx != 0 && dy != 0) continue; 
            
            // Calculamos posición futura
            var (nx, ny) = GetNextPositionWithWrap(ghost.X, ghost.Y, dir);

            if (!IsWall(nx, ny))
            {
                // Calculamos distancia al objetivo (Euclidiana)
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

        // Si encontró camino (o si está encerrado y bestDir sigue None)
        if (bestDir != Direction.None)
        {
            var (dx, dy) = GetDelta(bestDir);
            ghost.LastDirX = dx;
            ghost.LastDirY = dy;
            ghost.X = bestNextX;
            ghost.Y = bestNextY;
        }
        else
        {
            // Si está atrapado (cul-de-sac), permitimos reversa
             // Simplemente busca cualquier celda libre
        }
    }

    // 5. INTENSIDAD (LLAMAR ESTO AL COMER MONEDA)
    private void UpdateGameSpeed()
    {
        int coinsLeft = GameObjects.OfType<Coin>().Count();
        if (coinsLeft < 10) _gameTimer.Interval = TimeSpan.FromMilliseconds(130); // Frenético
        else if (coinsLeft < 50) _gameTimer.Interval = TimeSpan.FromMilliseconds(170); // Rápido
    }

    private void CheckGhostCollisions()
    {
        // Buscamos si hay colisión
        var hitGhost = GameObjects.OfType<Ghost>().FirstOrDefault(g => g.X == Player.X && g.Y == Player.Y);

        if (hitGhost is not null)
        {
            if (hitGhost.State == GhostState.Vulnerable)
            {
                // CASO 1: COMER FANTASMA
                Score += 200; // Puntos extra
                _audioPlayer.PlayWaka(); // O un sonido de "Eat Ghost" si tienes
                
                // Respawn del fantasma (lo mandamos a casa y lo curamos)
                hitGhost.X = hitGhost.StartX;
                hitGhost.Y = hitGhost.StartY;
                hitGhost.State = GhostState.Normal;
            }
            else
            {
                // CASO 2: FANTASMA NOS COME (Solo si es Normal)
                _audioPlayer.PlayDeath();
                Lives--;

                if (Lives > 0)
                {
                    ResetPositions();
                }
                else
                {
                    GameOver("GAME OVER");
                }
            }
        }
    }

// Helper para calcular posición futura sin mover el objeto real
    private (int x, int y) GetNewPosition(GameObject obj, Direction dir)
    {
        var (dx, dy) = GetDelta(dir);
        return (obj.X + dx, obj.Y + dy);
    }

    private bool IsWall(int x, int y)
    {
        return GameObjects.OfType<Wall>().Any(w => w.X == x && w.Y == y);
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
        var (nextX, nextY) = GetNextPositionWithWrap(Player.X, Player.Y, dir);
        return !IsWall(nextX, nextY);
    }

    // Mueve al jugador y come monedas
    private void MovePacman(Direction dir)
    {
        // 1. Obtenemos la coordenada futura (ya con el túnel aplicado)
        var (nextX, nextY) = GetNextPositionWithWrap(Player.X, Player.Y, dir);
        
        var cherry = GameObjects.OfType<Cherry>().FirstOrDefault(c => c.X == Player.X && c.Y == Player.Y);
        
        if (cherry is not null)
        {
            GameObjects.Remove(cherry);
            _audioPlayer.PlayWaka(); 

            // APLICAR EFECTO SEGÚN EL TIPO
            if (cherry.Type == CherryType.Blue)
            {
                // AZUL = VIDA EXTRA
                Lives++;
                // Opcional: _audioPlayer.PlayExtraLife();
            }
            else
            {
                // ROJA = FANTASMAS LENTOS
                _areGhostsSlowed = true;
                _slowGhostsTimer.Stop();
                _slowGhostsTimer.Start();
            }
        }

        // 2. Verificamos si esa posición es pared
        if (!IsWall(nextX, nextY))
        {
            // 3. Movemos
            Player.X = nextX;
            Player.Y = nextY;

            // 4. Lógica de comer moneda
            var coin = GameObjects.OfType<Coin>().FirstOrDefault(c => c.X == Player.X && c.Y == Player.Y);
        
            if (coin is not null)
            {
                GameObjects.Remove(coin);
                Score += 10;
                _audioPlayer.PlayWaka();

                // DETECCIÓN DE SUPER PÍLDORA
                if (coin.IsPowerPellet)
                {
                    ActivatePowerMode();
                    Score += 40; // Puntos extra por la píldora grande
                }
            }
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
    private void CheckWinCondition()
    {
        // Si NO quedan monedas en la lista de objetos
        if (!GameObjects.OfType<Coin>().Any())
        {
            _audioPlayer.PlayWin();
            GameOver("YOU WIN!");
        }
    }

    private void ResetPositions()
    {
        // Volver a Pacman al inicio
        Player.X = _startPacmanX;
        Player.Y = _startPacmanY;
        
        CurrentDirection = Direction.None;
        _nextDirection = Direction.None;
        IsMouthOpen = true; // Pacman revive con la boca abierta
        
        RequestRedraw?.Invoke();
    }

    private void GameOver(string message)
    {
        _gameTimer.Stop();
        IsGameOver = true;
        StatusMessage = message;
        _audioPlayer.StopAll(); // Detener música de fondo
    }
    private void InitializeGame()
    {
        string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "maps", "Level_layout1.txt");
        System.Diagnostics.Debug.WriteLine($"BUSCANDO MAPA EN: {path}");
       
        
        if (System.IO.File.Exists(path))
        {
            string[] lines = System.IO.File.ReadAllLines(path);
            LoadMapFromString(lines);
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("⚠ ERROR: NO SE ENCONTRÓ EL ARCHIVO. CARGANDO MAPA DE EMERGENCIA.");
        
            // Cargar un mapa hardcodeado para que no ganes instantáneamente
            string[] emergencyMap = {
                "WWWWWWWWWW",
                "W P    o W", // Al menos una moneda 'o' para no ganar al instante
                "WWWWWWWWWW"
            };
            LoadMapFromString(emergencyMap);
        }
    }

    private void LoadMapFromString(string[] lines)
    {
        try
        {
            GameObjects.Clear();
            // Reiniciamos Player para asegurarnos de que empezamos de cero
            Player = null; 
            
            _mapHeight = lines.Length;
            _mapWidth = lines[0].Length;

            for (int r = 0; r < lines.Length; r++)
            {
                for (int c = 0; c < lines[r].Length; c++)
                {
                    char tile = lines[r][c];

                    if (IsWallChar(tile))
                    {
                        GameObjects.Add(new Wall(c, r));
                    }
                    else if (tile == 'o' || tile == '.')
                    {
                        GameObjects.Add(new Coin(c, r, false)); // Moneda normal
                    }
                    else if (tile == 'O') // 'O' mayúscula es la Super Píldora
                    {
                        GameObjects.Add(new Coin(c, r, true)); // Super Píldora
                    }                    else if (tile == 'P' || tile == 'C')
                    {
                        // CORRECCIÓN: Solo creamos Pacman si no existe uno ya.
                        if (Player is null)
                        {
                            Player = new Pacman(c, r);
                            _startPacmanX = c;
                            _startPacmanY = r;
                            GameObjects.Add(Player);
                        }
                        else
                        {
                            // Si ya existe un Pacman, tratamos la casilla duplicada como vacía
                            // Opcional: System.Diagnostics.Debug.WriteLine($"Ignorado spawn extra en {c},{r}");
                        }
                    }
                    else if (char.IsDigit(tile))
                    {
                        // Asignamos personalidad según el número
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
                }
            }

            // Validación de seguridad: Si el mapa no tenía ni P ni C
            if (Player is null)
            {
                // Creamos uno por defecto en (1,1) para evitar crash
                Player = new Pacman(1, 1);
                GameObjects.Add(Player);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al procesar el mapa: {ex.Message}");
        }
    }

// Función auxiliar para detectar muros
    private bool IsWallChar(char c)
    {
        // Todos estos caracteres visuales son paredes sólidas
        return "QWAS|-><_".Contains(c); 
    }
    
    // Calcula la coordenada futura aplicando el "Efecto Túnel"
    private (int x, int y) GetNextPositionWithWrap(int currentX, int currentY, Direction dir)
    {
        var (dx, dy) = GetDelta(dir);
        int nextX = currentX + dx;
        int nextY = currentY + dy;

        // LÓGICA DE TÚNEL (Wrap-around)
        
        // Si se sale por la Izquierda -> Va a la Derecha
        if (nextX < 0)
        {
            nextX = _mapWidth - 1;
        }
        // Si se sale por la Derecha -> Va a la Izquierda
        else if (nextX >= _mapWidth)
        {
            nextX = 0;
        }

        // Si se sale por Arriba -> Va Abajo (Opcional, no suele usarse en Pacman clásico)
        if (nextY < 0)
        {
            nextY = _mapHeight - 1;
        }
        // Si se sale por Abajo -> Va Arriba
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
            // 1. Reiniciar el timer si ya estaba activo (para acumular tiempo)
            _powerModeTimer.Stop();
            _powerModeTimer.Start();

            // 2. Cambiar estado de todos los fantasmas
            foreach (var ghost in GameObjects.OfType<Ghost>())
            {
                ghost.State = GhostState.Vulnerable;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error activando poder: {ex.Message}");
        }
    }

    // Método para volver a la normalidad
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
                // 50% de probabilidad para cada tipo
                CherryType type = _random.Next(0, 2) == 0 ? CherryType.Red : CherryType.Blue;
                
                GameObjects.Add(new Cherry(rx, ry, type));
                break;
            }
        }
    }
}