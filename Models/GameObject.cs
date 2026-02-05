namespace PacmanAvalonia.Models;

/// <summary>
/// Represents the base entity for all objects within the game world, providing common positioning properties.
/// </summary>
public abstract class GameObject
{
    /// <summary>
    /// Gets or sets the horizontal coordinate of the object on the game grid.
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// Gets or sets the vertical coordinate of the object on the game grid.
    /// </summary>
    public int Y { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the GameObject class with the specified coordinates.
    /// </summary>
    /// <param name="x">The initial horizontal position.</param>
    /// <param name="y">The initial vertical position.</param>
    protected GameObject(int x, int y)
    {
        X = x;
        Y = y;
    }
}