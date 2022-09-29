namespace Model;

/// <summary>
/// A bokor osztálya
/// </summary>
public class Bush : Plant
{
    /// <summary>
    /// Inicializál egy új bokor példányt
    /// </summary>
    public Bush() : this(default) {}
    
    /// <summary>
    /// Inicializál egy új bokor példányt az adott pozícióra
    /// </summary>
    /// <param name="location">az adott pozíció</param>
    public Bush(GridPoint location) : base("Bokor", location, 1, 1)
    {
        Price = 10000;
    }
    
}