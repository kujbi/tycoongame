using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// Az XXL Extreme osztálya
    /// </summary>
    public class GiantSwing : Game
    {
        /// <summary>
        /// Inicializál egy új XXL Extreme példányt
        /// </summary>
        public GiantSwing() : this(new(0, 0)) {
            HappinessFactor = 3;
            BuildTime = 10;
            Price = 75000;
        }

        /// <summary>
        /// Inicializál egy új XXL Extreme példányt az adott pozícióra
        /// </summary>
        /// <param name="location">az adott pozíció</param>
        public GiantSwing(GridPoint location) : base("XXL Extreme",location, width: 1, height: 2) { 
            HappinessFactor = 3;
            BuildTime = 10;
            Price = 75000;
        }
    }
}
