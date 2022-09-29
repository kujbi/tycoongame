using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// A kísértetház osztálya
    /// </summary>
    public class HuntedHouse : Game
    {
        /// <summary>
        /// Inicializál egy új kísértetház példányt
        /// </summary>
        public HuntedHouse() : this(new(0, 0)) { 
            HappinessFactor = 2;
            BuildTime = 10;
            Price = 50000;
        }

        /// <summary>
        /// Inicializál egy új kísértetház példányt az adott pozícióra
        /// </summary>
        /// <param name="location">az adott pozíció</param>
        public HuntedHouse(GridPoint location) : base("Kísértetház",location, width: 2, height: 1) { 
            HappinessFactor = 2;
            BuildTime = 10;
            Price = 50000;
        }
    }
}
