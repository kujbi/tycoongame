using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// A dodgem osztálya
    /// </summary>
    public class Dodgem : Game
    {
        /// <summary>
        /// Inicializál egy új dodgem példányt
        /// </summary>
        public Dodgem() : this(new(0, 0)) { 
            HappinessFactor = 3;
            BuildTime = 10;
            Price = 75000;
        }

        /// <summary>
        /// Inicializál egy új dodgem példányt az adott pozícióra
        /// </summary>
        /// <param name="location">az adott pozíció</param>
        public Dodgem(GridPoint location) : base("Dodgem",location, width: 2, height: 1,activityDuration:10,new Point(0,0)) { 
            HappinessFactor = 3;
            BuildTime = 10;
            Price = 75000;
        }
    }
}
