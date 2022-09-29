using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// Út osztály
    /// </summary>
    public class Road : Facility
    {
        /// <summary>
        /// Inicializál egy új út példányt
        /// </summary>
        public Road() : this(new(0, 0)) { }

        /// <summary>
        /// Inicializál egy új út példányt a megadott helyen
        /// </summary>
        /// <param name="location">a megadott pozíció</param>
        public Road(GridPoint location) : base("Járda", location, width: 1, height: 1)
        {
            Price = 20000;
        }

        /// <summary>
        /// Az útkereséshez szükséges pozíció
        /// </summary>
        public override GridPoint LocationOnMap { get => Location; }

        /// <summary>
        /// Ez a metódus minden ticknél meghívódik. Ez felelős az automatikus folyamatok működéséért.
        /// </summary>
        public override void TimeAdvanced()
        {
            
        }
    }
}
