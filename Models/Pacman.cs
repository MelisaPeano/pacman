namespace PacmanAvalonia.Models.Entities;

public class Pacman : GameObject
{
    // Specific properties for the player.
    // Propiedades específicas para el jugador.
    public int Lives { get; set; } = 3;
    public int Score { get; set; } = 0;

    public Pacman(int x, int y) : base(x, y)
    {
    }
}