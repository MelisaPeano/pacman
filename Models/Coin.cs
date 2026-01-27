namespace PacmanAvalonia.Models;

public class Coin : GameObject
{
    public bool IsPowerUp { get; set; } // Para diferenciar puntos normales de los grandes

    public Coin(int x, int y, bool isPowerUp = false) : base(x, y)
    {
        IsPowerUp = isPowerUp;
    }
}