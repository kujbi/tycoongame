namespace Model;

/// <summary>
/// A fa osztálya
/// </summary>
public class Tree : Plant
{
    
    /// <summary>
    /// Inicializál egy új fa példányt
    /// </summary>
    public Tree() : this(default)
    {
    }

    
    /// <summary>
    /// Inicializál egy új fa példányt az adott pozícióra
    /// </summary>
    /// <param name="location">az adott pozíció</param>
    public Tree(GridPoint location) : base("Fa", location, 1, 2)
    {
    }
}