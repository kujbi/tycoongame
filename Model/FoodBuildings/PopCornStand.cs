using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// A popcorn stand osztálya
    /// </summary>
    public class PopCornStand : FoodBuilding
    {
        
        /// <summary>
        /// Inicializál egy új popcorn stand példányt
        /// </summary>
        public PopCornStand() : this(new GridPoint(0, 0)) { }

        /// <summary>
        /// Inicializál egy új popcorn stand példányt az adott pozícióra
        /// </summary>
        /// <param name="location">az adott pozíció</param>
        public PopCornStand(GridPoint location) : base("Popcorn árus", location, width: 1, height: 1) { 
            HungerFactor = 3;
            BuildTime = 5;
            Price = 25000;
        }
    }
}
