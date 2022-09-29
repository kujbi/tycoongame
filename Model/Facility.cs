using Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Model.Util;

namespace Model
{
    /// <summary>
    /// A létesítmények státuszai
    /// </summary>
    public enum FacilityStatus { Building, Working, Waiting, Broken, Repairing, WaitingForMaintenanceWorker }

    
    /// <summary>
    /// Egy olyan entitás, aminek van státusza.
    /// </summary>
    //Road
    [XmlInclude(typeof(Road))]
    //Games
    [XmlInclude(typeof(AimShooting))]
    [XmlInclude(typeof(Dodgem))]
    [XmlInclude(typeof(FerrisWheel))]
    [XmlInclude(typeof(GiantSwing))]
    [XmlInclude(typeof(HuntedHouse))]
    [XmlInclude(typeof(MiniTrain))]
    [XmlInclude(typeof(RollerCoaster))]
    //FoodFacility
    [XmlInclude(typeof(IceCreamStand))]
    [XmlInclude(typeof(PopCornStand))]
    [XmlInclude(typeof(Restaurant))]
    [XmlInclude((typeof(Tree)))]
    //Gate
    [XmlInclude(typeof(Gate))]
    //Plants
    [XmlInclude(typeof(Bush))]

    public abstract class Facility : Purchasable
    {

        //consturct
        protected Facility(string name, GridPoint location, int width, int height, int price = 1000) : base(name, location, width, height, price) { }

        /// <summary>
        /// A Status állapota
        /// </summary>
        //property
        [XmlIgnore]
        public State<FacilityStatus> StatusState { get; } = new(FacilityStatus.Building);
        
        /// <summary>
        /// A létesítmény státusza
        /// </summary>
        public FacilityStatus Status
        {
            get => StatusState.Value;
            set => StatusState.Value = value;
        }

        /// <summary>
        /// Az a pozíció, amit az útvonal-keresés használ
        /// </summary>
        public virtual GridPoint LocationOnMap { get; }

        
        public override bool Equals(object? obj)
        {
            if(obj is Facility that)
            {
                return this.Location == that.Location && this.Width == that.Width && this.Height == that.Height;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Location, Width, Height);
        }

        /// <summary>
        /// Megadja azokat a létesítményeket, amikhez vezet út ettől a létesítménytől
        /// </summary>
        /// <param name="map">A térkép ami meghatározza az útvonalakat</param>
        /// <returns>Az elérhető létesítmények</returns>
        public IEnumerable<Facility> GetBuildingsWithRoadToThis(Map map)
        {
            return map.GetBuildingsWithRoadToThis(this);
        }

        /// <summary>
        /// Megnézi, hogy az entitást le lehet-e tenni a megadott pozícióra
        /// </summary>
        /// <param name="location">a pozíció</param>
        /// <param name="park">a park, amibe le akarjuk helyezni</param>
        /// <returns>A lerakás eredménye</returns>
        public override PlacementResult CanBePlacedOnLocation(GridPoint location, Park park)
        {
            var facilityCoords = GetGridPoints(location);

            if(facilityCoords.Any(p => !p.IsInField(park.Size)))
            {
                return PlacementResult.OutOfField;
            }

            foreach(var currentFacility in park.Facilities)
            {
                if (ReferenceEquals(currentFacility, this)) continue;
                if (facilityCoords.Intersect(currentFacility.GetGridPoints()).Any())
                {
                    return PlacementResult.Conflict;
                }
            }

            return PlacementResult.Valid;
        }
    }
}
