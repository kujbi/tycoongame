using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// A növények ősosztálya
    /// </summary>
    public abstract class Plant : Facility
    {
        /// <summary>
        /// Inicializál egy növényt
        /// </summary>
        /// <param name="name">a növény neve</param>
        /// <param name="location">a növény pozíciója a pályán</param>
        /// <param name="width">a növény szélessége</param>
        /// <param name="height">a növény magassága</param>
        protected Plant(string name, GridPoint location, int width, int height) : base(name, location, width, height)
        {

        }

        /// <summary>
        /// Megadja, hogy a növény mennyivel növeli a látogatók boldogságát
        /// </summary>
        public int HappinessFactor { get; set; } = 1;

        /// <summary>
        /// Ez a metódus minden ticknél meghívódik. Ez felelős az automatikus folyamatok működéséért.
        /// </summary>
        public override void TimeAdvanced()
        {
            
        }
    }
}
