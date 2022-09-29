namespace Model
{
    /// <summary>
    /// A jégkrém stand osztálya
    /// </summary>
    public class IceCreamStand : FoodBuilding
    {
        /// <summary>
        /// Inicializál egy új jégkrém stand példányt
        /// </summary>
        public IceCreamStand() : this(new GridPoint(0, 0)) { }
        
        /// <summary>
        /// Inicializál egy új jégkrém stand példányt az adott pozícióra
        /// </summary>
        /// <param name="location">az adott pozíció</param>
        public IceCreamStand(GridPoint location) : base("Jégkrém árus", location, width: 1, height: 1) { 
            HungerFactor = 3;
            BuildTime = 5;
            Price = 25000;
        }

    }
}
