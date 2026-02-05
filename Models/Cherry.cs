using PacmanAvalonia.Models.Entities;
using PacmanAvalonia.Models.enums;

namespace PacmanAvalonia.Models.Entities;

/// <summary>
/// Represents a bonus fruit item in the game world, inheriting from GameObject.
/// </summary>
public class Cherry : GameObject
{
    /// <summary>
    /// Gets the specific type of the cherry, determining its effect on the game state.
    /// </summary>
    public CherryType Type { get; private set; }

    /// <summary>
    /// Initializes a new instance of the Cherry class with a specified position and type.
    /// </summary>
    /// <param name="x">The X coordinate on the game map.</param>
    /// <param name="y">The Y coordinate on the game map.</param>
    /// <param name="type">The specific variant of the cherry (e.g., Red or Blue).</param>
    public Cherry(int x, int y, CherryType type) : base(x, y) 
    {
        Type = type;
    }
}