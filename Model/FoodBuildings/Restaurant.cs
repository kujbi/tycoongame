using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// Az étterem osztálya
    /// </summary>
    public class Restaurant : FoodBuilding
    {
        /// <summary>
        /// Inicializál egy új étterem példányt
        /// </summary>
        public Restaurant() : this(new GridPoint(0, 0)) { }

        /// <summary>
        /// Inicializál egy új étterem példányt az adott pozícióra
        /// </summary>
        /// <param name="location">az adott pozíció</param>
        public Restaurant(GridPoint location) : base("Étterem", location, width: 2, height: 1)
        {
            HungerFactor = 5;
            BuildTime = 10;
            Price=75000;
        }


    }
}
