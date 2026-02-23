using System;
using System.Collections.Generic;
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

/// <summary>
/// Responsible for rendering the game state onto the Avalonia Canvas.
/// Handles asset loading, sprite slicing, and drawing all game entities frame by frame.
/// </summary>
public class GameRenderer
{
    private readonly Canvas _canvas;
    private const int TileSize = 30;

    private readonly IImage _pacmanOpen;
    private readonly IImage _pacmanClosed;
    private readonly IImage _coinImg;
    private readonly IImage _powerImg;
    
    private readonly IImage _ghostBlinky;
    private readonly IImage _ghostPinky;
    private readonly IImage _ghostInky;
    private readonly IImage _ghostClyde;
    private readonly IImage _ghostVulnerable; 
    private readonly IImage _cherryRedImg;
    private readonly IImage _cherryBlueImg;

    private readonly IImage _wallHor;
    private readonly IImage _wallVer;
    private readonly IImage _wallTopLeft;
    private readonly IImage _wallTopRight;
    private readonly IImage _wallBotLeft;
    private readonly IImage _wallBotRight;
    private readonly IImage _wallEndTop;    
    private readonly IImage _wallEndBot;    
    private readonly IImage _wallEndLeft;
    private readonly IImage _wallEndRight;
    private readonly IImage _ghostDoorImg;
    /// <summary>
    /// Initializes a new instance of the GameRenderer class.
    /// Loads all necessary image assets and prepares sprite cutouts.
    /// </summary>
    /// <param name="canvas">The Avalonia Canvas control where the game will be drawn.</param>
    public GameRenderer(Canvas canvas)
    {
        _canvas = canvas;

        try
        {
            var pacmanSheet = LoadBitmap("Pacman.png");
            var blinkySheet = LoadBitmap("GhostBlinky.png");
            var pinkySheet = LoadBitmap("GhostPinky.png");
            var clydeSheet = LoadBitmap("GhostClyde.png");
            var vulnerableSheet = LoadBitmap("blueGhost.png");
            var inkySheet = LoadBitmap("GhostBlue.png"); 
            
            _cherryRedImg = LoadBitmap("Cherry.png");
            _cherryBlueImg = LoadBitmap("CherryInverted.png");
            _coinImg = LoadBitmap("Coin.png");
            
            var coinSheet = LoadBitmap("Coin.png"); 
            
            int coinFrameWidth = coinSheet.PixelSize.Width / 7;
            int coinFrameHeight = coinSheet.PixelSize.Height;

            _coinImg = new CroppedBitmap(coinSheet, new PixelRect(0, 0, coinFrameWidth, coinFrameHeight));
            _powerImg = LoadBitmap("Munchie.png");

            int pmW = pacmanSheet.PixelSize.Width / 3;
            int pmH = pacmanSheet.PixelSize.Height / 4;
            _pacmanOpen = new CroppedBitmap(pacmanSheet, new PixelRect(0, 0, pmW, pmH));
            _pacmanClosed = new CroppedBitmap(pacmanSheet, new PixelRect(pmW * 1, 0, pmW, pmH));

            int gW = blinkySheet.PixelSize.Width / 2;
            int gH = blinkySheet.PixelSize.Height / 4;
            _ghostBlinky = new CroppedBitmap(blinkySheet, new PixelRect(0, 0, gW, gH));
            _ghostPinky = new CroppedBitmap(pinkySheet, new PixelRect(0, 0, gW, gH));
            _ghostClyde = new CroppedBitmap(clydeSheet, new PixelRect(0, 0, gW, gH));
            
            int vulW = vulnerableSheet.PixelSize.Width / 8; 
            int vulH = vulnerableSheet.PixelSize.Height; 
            _ghostVulnerable = new CroppedBitmap(vulnerableSheet, new PixelRect(0, 0, vulW, vulH));
            
            _wallHor = LoadBitmap("Walls/HorizontalWallTile.png");
            _wallVer = LoadBitmap("Walls/VerticalWallTile.png");
            
            _wallTopLeft = LoadBitmap("Walls/TopLeftCornerWall.png");
            _wallTopRight = LoadBitmap("Walls/TopRightCornerWall.png");
            _wallBotLeft = LoadBitmap("Walls/BottomLeftCornerWall.png");
            _wallBotRight = LoadBitmap("Walls/BottomRightCornerWall.png");
            
            _wallEndTop = LoadBitmap("Walls/UpperWallEnd.png");
            _wallEndBot = LoadBitmap("Walls/BottomWallEnd.png");
            _wallEndLeft = LoadBitmap("Walls/LeftWallEnd.png");
            _wallEndRight = LoadBitmap("Walls/RightWallEnd.png");
            
            _ghostDoorImg = LoadBitmap("ghostDoor.png");
            
            _ghostInky = new CroppedBitmap(inkySheet, new PixelRect(0, 0, gW, gH));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Asset Loading Error: {ex.Message}");
            var empty = new WriteableBitmap(new PixelSize(1,1), new Vector(96,96), PixelFormat.Rgba8888);
            _cherryRedImg = empty;
            _cherryBlueImg = empty;
            _pacmanOpen = empty;
            _pacmanClosed = empty;
            _ghostBlinky = empty;
            _ghostPinky = empty;
            _ghostInky = empty;
            _ghostClyde = empty;
            _ghostVulnerable = empty;
            _coinImg = empty;
            _powerImg = empty;
            _ghostDoorImg = empty;
            
            _wallHor = _wallVer = _wallTopLeft = _wallTopRight = _wallBotLeft = _wallBotRight = 
                _wallEndTop = _wallEndBot = _wallEndLeft = _wallEndRight = empty;
        }
    }

   /// <summary>
    /// Clears the canvas and redraws the current state of the game based on the ViewModel data.
    /// </summary>
    /// <param name="viewModel">The ViewModel containing the current positions and states of all game objects.</param>
    public void Draw(GameViewModel viewModel)
    {
        if (viewModel is null)
        {
            return;
        }

        _canvas.Children.Clear();
        _canvas.Background = Brushes.Black;
        

        foreach (var obj in viewModel.GameObjects)
        {
            Control? visual = null;
            double x = obj.X * TileSize;
            double y = obj.Y * TileSize;

            switch (obj)
            {
                case Wall w:
                    IImage wallSprite = null;

                    switch (w.Type)
                    {
                        case '-': 
                            wallSprite = _wallHor; 
                            break;
                        case '|': 
                            wallSprite = _wallVer; 
                            break;
        
                        case 'Q': 
                            wallSprite = _wallTopLeft; 
                            break;
                        case 'W': 
                            wallSprite = _wallTopRight; 
                            break;
        
                        case 'A': 
                            wallSprite = _wallBotLeft; 
                            break;
                        case 'S': 
                            wallSprite = _wallBotRight; 
                            break;
                        case 'U' :
                            wallSprite = _wallEndTop;
                            break;
                        case 'L' : 
                            wallSprite = _wallEndLeft;
                            break;
                        case 'R' :
                            wallSprite = _wallEndRight;
                            break;
                        case 'D': 
                            wallSprite = _wallEndBot;
                            break;

                        default: 
                            wallSprite = _wallHor; 
                            break;
                    }
    
                    if (wallSprite != null)
                    {
                        visual = new Image 
                        { 
                            Source = wallSprite, 
                            Width = TileSize, 
                            Height = TileSize 
                        };
                    }
                    break;
                case GhostDoor:
                    visual = new Image 
                    { 
                        Source = _ghostDoorImg, 
                        Width = TileSize, 
                        Height = TileSize 
                    };
                    break;
                
                case Pacman:
                    IImage spriteToShow = viewModel.IsMouthOpen ? _pacmanOpen : _pacmanClosed;
                    var pacImage = new Image 
                    { 
                        Source = spriteToShow, 
                        Width = TileSize, 
                        Height = TileSize 
                    };

                    double angle = 0;
                    switch (viewModel.CurrentDirection)
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
                    visual = new Image 
                    { 
                        Source = coin.IsPowerPellet ? _powerImg : _coinImg, 
                        Width = size, 
                        Height = size 
                    };
                    x += (TileSize / 2) - (size / 2);
                    y += (TileSize / 2) - (size / 2);
                    break;

                case Ghost ghost:
                    IImage ghostSprite;
                    if (ghost.State == GhostState.Vulnerable)
                    {
                        ghostSprite = _ghostVulnerable; 
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
                    visual = new Image { Source = img, Width = TileSize, Height = TileSize };
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

    private Bitmap LoadBitmap(string fileName)
    {
        var uri = new Uri($"avares://PacmanAvalonia/Assets/{fileName}");
        return new Bitmap(AssetLoader.Open(uri));
    }
}