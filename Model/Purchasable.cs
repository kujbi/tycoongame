namespace Model;

/// <summary>
/// A katalógusból megvásárolható elemek ősosztálya
/// </summary>
public abstract class Purchasable : Entity
{
    /// <summary>
    /// Inicializál egy új megvásárolható elemet
    /// </summary>
    /// <param name="name">Az elem neve</param>
    /// <param name="location">Az elem pozíciója</param>
    /// <param name="width">Az elem szélessége</param>
    /// <param name="height">Az elem magassága</param>
    /// <param name="price">Az elem ára</param>
    protected Purchasable(string name, GridPoint location, int width, int height, int price) : base(location, width, height)
    {
        Price = price;
        Name = name;
    }

    /// <summary>
    /// A megvásárolható elem ára
    /// </summary>
    public int Price { get; set; }
    
    /// <summary>
    /// A megvásárolható elem neve
    /// </summary>
    public string Name { get; }
}