using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Xml.Serialization;
using Model.Util;

namespace Model
{

    /// <summary>
    /// Irányok
    /// </summary>
    public enum Direction { Up, Down, Left, Right, NoDirection }
    
    /// <summary>
    /// A parkban lehelyezhető objektumok ősosztálya.
    /// </summary>
    public abstract class Entity
    {
        protected readonly Int32 MINUS_HAPPINESS_POINTS_WHEN_HUNGRY = 3;
        protected readonly Int32 HUNGERPOINT = 1;
        protected readonly Int32 HAPPINESSPOINT = 1;
        protected readonly Int32 MINUS_HAPPINESS_POINT_WHEN_WAITING = 2;
        protected readonly Int32 HAPPINESS_WHEN_IN_FOODBUILDING = 2;
        public static readonly Int32 VISITOR_STARTING_MONEY_AMOUNT = 10000;

        /// <summary>
        /// A Location állapota.
        /// </summary>
        [XmlIgnore]
        public State<GridPoint> LocationState { get; }

        /// <summary>
        /// Az entitás pozíciója a pályán.
        /// </summary>
        public GridPoint Location
        {
            get => LocationState.Value;
            set => LocationState.Value = value;
        }

        /// <summary>
        /// Az entitás magassága
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Az entitás szélessége
        /// </summary>
        public int Width { get; set; }
        
        protected Entity(GridPoint location, int width, int height)
        {
            LocationState = new State<GridPoint>(location);
            Height = height;
            Width = width;
        }

        /// <summary>
        /// Ez a metódus minden tickben meghívódik
        /// </summary>
        public abstract void TimeAdvanced();
        
        protected List<GridPoint> GetGridPoints(GridPoint location)
        {
            var list = new List<GridPoint>();

            for (int i = location.Column; i < location.Column + Width; i++)
            {
                for (int j = location.Row; j < location.Row + Height; j++)
                {
                    list.Add(new GridPoint(j, i));
                }
            }

            return list;
        }

        /// <summary>
        /// Megadja az entitás pontjainak összességét a pályán.
        /// </summary>
        /// <returns>Az pontok összessége</returns>
        public List<GridPoint> GetGridPoints()
        {
            return GetGridPoints(Location);
        }

        /// <summary>
        /// Megnézi, hogy az entitást le lehet-e tenni a megadott pozícióra
        /// </summary>
        /// <param name="location">a pozíció</param>
        /// <param name="park">a park, amibe le akarjuk helyezni</param>
        /// <returns>a lerakás eredménye</returns>
        public abstract PlacementResult CanBePlacedOnLocation(GridPoint location, Park park);
    }
}
