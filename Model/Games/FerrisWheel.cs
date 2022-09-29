using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// A óriáskerék osztálya
    /// </summary>
    public class FerrisWheel : Game
    {
        /// <summary>
        /// Inicializál egy új óriáskerék példányt
        /// </summary>
        public FerrisWheel() : this(new GridPoint(0, 0)) { 
            HappinessFactor = 1;
            BuildTime = 20;
            Price = 75000;
        }

        /// <summary>
        /// Inicializál egy új óriáskerék példányt az adott pozícióra
        /// </summary>
        /// <param name="location">az adott pozíció</param>
        public FerrisWheel(GridPoint location): base("Óriáskerék",location: location, width: 2, height: 2,activityDuration: 20,new Point(0,1)) { 
            HappinessFactor = 1;
            BuildTime = 20;
            Price = 75000;
        }
    }
}
