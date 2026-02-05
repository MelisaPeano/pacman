namespace PacmanAvalonia.Models.Entities;

/// <summary>
/// Represents the player-controlled character in the game.
/// </summary>
public class Pacman : GameObject
{
    /// <summary>
    /// Gets or sets the number of lives remaining for the player. Default is 3.
    /// </summary>
    public int Lives { get; set; } = 3;

    /// <summary>
    /// Gets or sets the current score accumulated by the player. Default is 0.
    /// </summary>
    public int Score { get; set; } = 0;

    /// <summary>
    /// Initializes a new instance of the Pacman class at the specified starting position.
    /// </summary>
    /// <param name="x">The initial horizontal coordinate.</param>
    /// <param name="y">The initial vertical coordinate.</param>
    public Pacman(int x, int y) : base(x, y)
    {
    }
}