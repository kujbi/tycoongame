using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// A kisvasút osztálya
    /// </summary>
    public class MiniTrain : Game
    {
        
        /// <summary>
        /// Inicializál egy új kisvasút példányt
        /// </summary>
        public MiniTrain() : this(new(0, 0)) { 
            HappinessFactor = 1;
            BuildTime = 10;
            Price = 25000;
        }

        /// <summary>
        /// Inicializál egy új kisvasút példányt az adott pozícióra
        /// </summary>
        /// <param name="location">az adott pozíció</param>
        public MiniTrain(GridPoint location) : base("Kisvasút",location, width: 2, height: 1) { 
            HappinessFactor = 1;
            BuildTime = 10;
            Price = 50000;
        }
    }
}
