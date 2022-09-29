using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// A hullámvasút osztálya
    /// </summary>
    public class RollerCoaster : Game
    {
        
        /// <summary>
        /// Inicializál egy új hullámvasút példányt
        /// </summary>
        public RollerCoaster() : this(new(0, 0)) { 
            HappinessFactor = 3;
            BuildTime = 10;
            Price = 100000;
        }

        /// <summary>
        /// Inicializál egy új hullámvasút példányt az adott pozícióra
        /// </summary>
        /// <param name="location">az adott pozíció</param>
        public RollerCoaster(GridPoint location) : base("Hullámvasút", location, width: 2, height: 1) { 
            HappinessFactor = 3;
            BuildTime = 10;
            Price = 200000;
        }
    }
}
