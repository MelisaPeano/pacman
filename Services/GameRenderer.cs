using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging; 
using Avalonia.Platform;      
using PacmanAvalonia.Models;
using PacmanAvalonia.Models.Entities;
using PacmanAvalonia.Models.enums;
using PacmanAvalonia.ViewModels;

namespace PacmanAvalonia.Services;

public class GameRenderer
{
    private readonly Canvas _canvas;
    private const int TileSize = 30;

    // --- IMÁGENES ---
    private readonly IImage _pacmanOpen;
    private readonly IImage _pacmanClosed;
    private readonly IImage _coinImg;
    private readonly IImage _powerImg;
    
    // Fantasmas específicos
    private readonly IImage _ghostBlinky;
    private readonly IImage _ghostPinky;
    private readonly IImage _ghostInky; // blueGhost.png (Tira horizontal)
    private readonly IImage _ghostClyde;
    private readonly IImage _ghostVulnerable; 
    private readonly IImage _cherryRedImg;
    private readonly IImage _cherryBlueImg;

    public GameRenderer(Canvas canvas)
    {
        _canvas = canvas;

        try
        {
            // --- CARGA DE ASSETS ---
            var pacmanSheet = LoadBitmap("Pacman.png");
            
            // Fantasmas
            var blinkySheet = LoadBitmap("GhostBlinky.png");
            var pinkySheet = LoadBitmap("GhostPinky.png");
            var clydeSheet = LoadBitmap("GhostClyde.png");
            var vulnerableSheet = LoadBitmap("GhostBlue.png"); // El asustado clásico
            var inkySheet = LoadBitmap("blueGhost.png"); 
            _cherryRedImg = LoadBitmap("Cherry.png");
            _cherryBlueImg = LoadBitmap("CherryInverted.png");

            // --- RECORTES ---

            // 1. PACMAN (Grilla 3x4)
            int pmW = pacmanSheet.PixelSize.Width / 3;
            int pmH = pacmanSheet.PixelSize.Height / 4;
            _pacmanOpen = new CroppedBitmap(pacmanSheet, new PixelRect(0, 0, pmW, pmH));
            _pacmanClosed = new CroppedBitmap(pacmanSheet, new PixelRect(pmW * 1, 0, pmW, pmH));

            // 2. FANTASMAS CLÁSICOS (Grilla 2x4)
            // Blinky, Pinky, Clyde y Vulnerable tienen la misma estructura de cuadrícula
            int gW = blinkySheet.PixelSize.Width / 2;
            int gH = blinkySheet.PixelSize.Height / 4;

            _ghostBlinky = new CroppedBitmap(blinkySheet, new PixelRect(0, 0, gW, gH));
            _ghostPinky = new CroppedBitmap(pinkySheet, new PixelRect(0, 0, gW, gH));
            _ghostClyde = new CroppedBitmap(clydeSheet, new PixelRect(0, 0, gW, gH));
            _ghostVulnerable = new CroppedBitmap(vulnerableSheet, new PixelRect(0, 0, gW, gH));

            // 3. INKY (blueGhost.png) - CORRECCIÓN
            // Según tu imagen, es una TIRA HORIZONTAL de 8 fantasmas.
            // Ancho = Total / 8. Alto = Total.
            int inkyW = inkySheet.PixelSize.Width / 8;
            int inkyH = inkySheet.PixelSize.Height; 

            // Tomamos el primero de la izquierda
            _ghostInky = new CroppedBitmap(inkySheet, new PixelRect(0, 0, inkyW, inkyH));

            // 4. OBJETOS
            _coinImg = LoadBitmap("Dot.png");
            _powerImg = LoadBitmap("Munchie.png");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR CARGANDO ASSETS: {ex.Message}");
            var empty = new WriteableBitmap(new PixelSize(1,1), new Vector(96,96), PixelFormat.Rgba8888);
            _cherryRedImg = _cherryBlueImg = empty;
            _pacmanOpen = _pacmanClosed = _ghostBlinky = _ghostPinky = _ghostInky = _ghostClyde = _ghostVulnerable = _coinImg = _powerImg = empty;
        }
    }

    private Bitmap LoadBitmap(string fileName)
    {
        var uri = new Uri($"avares://PacmanAvalonia/Assets/{fileName}");
        return new Bitmap(AssetLoader.Open(uri));
    }

    public void Draw(GameViewModel vm)
    {
        if (vm is null) return;

        _canvas.Children.Clear();
        _canvas.Background = Brushes.Black;

        foreach (var obj in vm.GameObjects)
        {
            Control? visual = null;
            double x = obj.X * TileSize;
            double y = obj.Y * TileSize;

            switch (obj)
            {
                case Wall:
                    visual = new Avalonia.Controls.Shapes.Rectangle
                    {
                        Width = TileSize, Height = TileSize,
                        Fill = Brushes.Transparent, Stroke = Brushes.DarkBlue, StrokeThickness = 2
                    };
                    break;
                
                case Pacman:
                    IImage spriteToShow = vm.IsMouthOpen ? _pacmanOpen : _pacmanClosed;
                    var pacImage = new Image { Source = spriteToShow, Width = TileSize, Height = TileSize };

                    double angle = 0;
                    switch (vm.CurrentDirection)
                    {
                        case Direction.Right: angle = 0; break;
                        case Direction.Down:  angle = 90; break;
                        case Direction.Left:  angle = 180; break;
                        case Direction.Up:    angle = 270; break;
                    }
                    if (angle > 0)
                    {
                        pacImage.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
                        pacImage.RenderTransform = new RotateTransform(angle);
                    }
                    visual = pacImage;
                    break;

                case Coin coin:
                    double size = coin.IsPowerPellet ? TileSize : 10; 
                    visual = new Image { Source = coin.IsPowerPellet ? _powerImg : _coinImg, Width = size, Height = size };
                    x += (TileSize / 2) - (size / 2);
                    y += (TileSize / 2) - (size / 2);
                    break;

                case Ghost ghost:
                    // Seleccionar imagen según estado y tipo
                    IImage ghostSprite;
                    
                    if (ghost.State == GhostState.Vulnerable)
                    {
                        ghostSprite = _ghostVulnerable; // GhostBlue.png (Grid)
                    }
                    else
                    {
                        ghostSprite = ghost.Type switch
                        {
                            GhostType.Blinky => _ghostBlinky,
                            GhostType.Pinky => _ghostPinky,
                            GhostType.Inky => _ghostInky,   
                            GhostType.Clyde => _ghostClyde,
                            _ => _ghostBlinky
                        };
                    }

                    visual = new Image { Source = ghostSprite, Width = TileSize, Height = TileSize };
                    break;
                case Cherry cherry:
                    IImage img = cherry.Type == CherryType.Red ? _cherryRedImg : _cherryBlueImg;
                    
                    visual = new Image 
                    { 
                        Source = img, 
                        Width = TileSize, 
                        Height = TileSize 
                    };
                    break;
            }

            if (visual is not null)
            {
                Canvas.SetLeft(visual, x);
                Canvas.SetTop(visual, y);
                _canvas.Children.Add(visual);
            }
        }
    }
}