namespace PacmanAvalonia.Models;

public abstract class GameObject
{
    public int X { get; set; }
    public int Y { get; set; }
    
    protected GameObject(int x, int y)
    {
        X = x;
        Y = y;
    }
}