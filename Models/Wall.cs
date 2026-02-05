namespace PacmanAvalonia.Models.Entities;

/// <summary>
/// Wall in the game
/// </summary>
public class Wall : GameObject
{
    /// <summary>
    /// position
    /// </summary>
    /// <param name="x">represents the x position in the layout</param>
    /// <param name="y">represent the y position in the layout</param>
    public Wall(int x, int y) : base(x, y) { }
}