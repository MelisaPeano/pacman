namespace PacmanAvalonia.Models;

/// <summary>
/// Represents a collectible entity within the game world, such as a standard dot or a power pellet.
/// </summary>
public class Coin : GameObject
{
    private bool _isPowerPellet =  false;
    /// <summary>
    /// Gets or sets a value indicating whether this collectible is a power pellet.
    /// </summary>
    public bool IsPowerPellet { get => _isPowerPellet; } 

    /// <summary>
    /// Initializes a new instance of the Coin class at the specified coordinates.
    /// </summary>
    /// <param name="x">The horizontal coordinate on the game grid.</param>
    /// <param name="y">The vertical coordinate on the game grid.</param>
    /// <param name="isPower">If set to true, this coin functions as a power pellet; otherwise, it is a standard dot.</param>
    public Coin(int x, int y, bool isPower = false) : base(x, y) 
    {
        _isPowerPellet = isPower;
    }
}