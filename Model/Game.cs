using System.Drawing;
using System.Xml.Serialization;

namespace Model
{
    /// <summary>
    /// A játékok ősosztálya
    /// </summary>
    [XmlInclude(typeof(AimShooting))]
    [XmlInclude(typeof(Dodgem))]
    public abstract class Game : Building
    {
        protected Game(string name, GridPoint location, int width, int height, int activityDuration=15, Point gateOffset = default) : base(name, location, width, height,activityDuration, gateOffset) { }
        
        /// <summary>
        /// Megadja, hogy a játék mennyivel növeli a látogatók boldogságát
        /// </summary>
        public int HappinessFactor { get; set; }

        

        
    }


}
