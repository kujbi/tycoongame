using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// A Célba lövés osztálya
    /// </summary>
    public class AimShooting : Game
    {
        /// <summary>
        /// Inicializál egy új célba lövés példányt
        /// </summary>
        public AimShooting() : this(new(0,0)) { 
            HappinessFactor = 2;
            BuildTime = 10;
            Price = 75000;
        }
        
        /// <summary>
        /// Inicializál egy új célba lövés példányt az adott pozícióra
        /// </summary>
        /// <param name="location">az adott pozíció</param>
        public AimShooting(GridPoint location) : base("Célba lövés", location, width: 2, height: 1, activityDuration:20, gateOffset: new Point(0,0) ) { 
            HappinessFactor = 2;
            BuildTime = 10;
            Price = 75000;
        }
    }
}
