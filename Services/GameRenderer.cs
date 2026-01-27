namespace PacmanAvalonia.Services;

using PacmanAvalonia.Models;
using PacmanAvalonia.Models.Entities;


using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using System.Collections.Generic;



public class GameRenderer
{
    private readonly Canvas _canvas;
    private const int TileSize = 30;

    public GameRenderer(Canvas canvas)
    {
        _canvas = canvas;
    }

    public void Draw(IEnumerable<GameObject> objects)
    {
        _canvas.Children.Clear();

        foreach (var obj in objects)
        {
            double x = obj.X * TileSize;
            double y = obj.Y * TileSize;
            
            Shape? shape = null;

            switch (obj)
            {
                case Wall:
                    shape = new Rectangle 
                    { 
                        Width = TileSize, Height = TileSize, Fill = Brushes.Blue 
                    };
                    break;
                
                case Pacman:
                    shape = new Ellipse 
                    { 
                        Width = TileSize - 4, Height = TileSize - 4, Fill = Brushes.Yellow 
                    };
                    x += 2; y += 2;
                    break;
                
                case Coin:
                    shape = new Ellipse 
                    { 
                        Width = 6, Height = 6, Fill = Brushes.White 
                    };
                    x += (TileSize / 2) - 3; y += (TileSize / 2) - 3; 
                    break;
            }

            if (shape != null)
            {
                Canvas.SetLeft(shape, x);
                Canvas.SetTop(shape, y);
                _canvas.Children.Add(shape);
            }
        }
    }
}