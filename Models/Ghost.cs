using PacmanAvalonia.Models.enums;

namespace PacmanAvalonia.Models.Entities;

/// <summary>
/// Represents a ghost enemy entity in the game, tracking its state, personality type, and movement history.
/// </summary>
public class Ghost : GameObject
{
    /// <summary>
    /// Gets or sets the current behavioral state of the ghost (e.g., Normal, Vulnerable).
    /// </summary>
    public GhostState State { get; set; } = GhostState.Normal;

    /// <summary>
    /// Gets the personality type of the ghost, which determines its visual appearance and AI behavior.
    /// </summary>
    public GhostType Type { get; private set; }
    
    /// <summary>
    /// Gets or sets the X component of the last movement direction to prevent immediate 180-degree turns.
    /// </summary>
    public int LastDirX { get; set; } = 0;

    /// <summary>
    /// Gets or sets the Y component of the last movement direction to prevent immediate 180-degree turns.
    /// </summary>
    public int LastDirY { get; set; } = 0;

    /// <summary>
    /// Gets the initial X coordinate where the ghost spawned, used for resetting positions.
    /// </summary>
    public int StartX { get; private set; }

    /// <summary>
    /// Gets the initial Y coordinate where the ghost spawned, used for resetting positions.
    /// </summary>
    public int StartY { get; private set; }

    /// <summary>
    /// Initializes a new instance of the Ghost class with a specific position and personality type.
    /// </summary>
    /// <param name="x">The initial horizontal position.</param>
    /// <param name="y">The initial vertical position.</param>
    /// <param name="type">The specific identity of the ghost (e.g., Blinky, Pinky).</param>
    public Ghost(int x, int y, GhostType type) : base(x, y) 
    {
        StartX = x;
        StartY = y;
        Type = type;
    }
}