using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Xml.Serialization;
using Model.EventArguments;
using Model.Util;

namespace Model
{
    /// <summary>
    /// A karbantartó státuszai
    /// </summary>
	public enum WorkerStatus { Wandering, Working, WanderingToWork }
    
    /// <summary>
    /// A karbantartó osztály
    /// </summary>
	public class MaintenanceWorker : Purchasable
	{
        private IList<Facility>? _path;

        private int _repairTimeAdvanced;

        /// <summary>
        /// Inicializál egy új karbantartó példányt
        /// </summary>
        public MaintenanceWorker(): this(new GridPoint(0,0)) { }

        /// <summary>
        /// Inicializál egy új karbantartót a megadott pozícióban
        /// </summary>
        /// <param name="location">a pozíció a pályán</param>
        public MaintenanceWorker(GridPoint location) : base("Karbantartó", location, 1, 1, 1000) 
        {
            Direction = Direction.Up;
            _repairTimeAdvanced = 0;
        }

        private Direction Direction { get; set; }

        private Facility? CurrentBuilding { get; set; }


        /// <summary>
        /// A Status állapota
        /// </summary>
        [XmlIgnore] 
        public State<WorkerStatus> StatusState { get; } = new(WorkerStatus.Wandering);

        /// <summary>
        /// A karbantartó státusza
        /// </summary>
        public WorkerStatus Status
        {
            get => StatusState.Value; 
            set => StatusState.Value = value;
        }
        
        private void StepOneForward()
        {
            _path!.Remove(CurrentBuilding!);
            if (_path!.Count == 0)
            {
                switch (Status)
                {
                    case WorkerStatus.Wandering:
                        ChooseDestination();
                        break;
                    case WorkerStatus.WanderingToWork:
                        EnterToRepairBuilding((Building)CurrentBuilding!);
                        break;
                }
            }
            else
            {
                if (_path.First().LocationOnMap.Row < this.Location.Row && _path.First().LocationOnMap.Column == this.Location.Column)
                    Direction = Direction.Up;
                else if (_path.First().LocationOnMap.Row > this.Location.Row && _path.First().LocationOnMap.Column == this.Location.Column)
                    Direction = Direction.Down;
                else if (_path.First().LocationOnMap.Row == this.Location.Row && _path.First().LocationOnMap.Column > this.Location.Column)
                    Direction = Direction.Right;
                else
                    Direction = Direction.Left;

                switch (Direction)
                {
                    case Direction.Left:
                        if (WorkerMoving != null)
                        {
                            WorkerMoving(this, new MoveEventArgs(new GridPoint(Location.Row, Location.Column - 1)));
                        }
                        Location = new GridPoint(Location.Row, Location.Column - 1);
                        break;
                    case Direction.Right:
                        if (WorkerMoving != null)
                        {
                            WorkerMoving(this, new MoveEventArgs(new GridPoint(Location.Row, Location.Column + 1)));
                        }
                        Location = new GridPoint(Location.Row, Location.Column + 1);
                        break;
                    case Direction.Up:
                        if (WorkerMoving != null)
                        {
                            WorkerMoving(this, new MoveEventArgs(new GridPoint(Location.Row + 1, Location.Column)));
                        }
                        Location = new GridPoint(Location.Row - 1, Location.Column);
                        break;
                    case Direction.Down:
                        if (WorkerMoving != null)
                        {
                            WorkerMoving(this, new MoveEventArgs(new GridPoint(Location.Row - 1, Location.Column)));
                        }
                        Location = new GridPoint(Location.Row + 1, Location.Column);
                        break;
                }

                CurrentBuilding = _path.First();
            }
        }
        
        private void EnterToRepairBuilding(Building buildingToRepair)
        {
            buildingToRepair.StartRepair();
            Status = WorkerStatus.Working;
        }

        /// <summary>
        /// Ez a metódus minden ticknél meghívódik. Ez felelős az automatikus folyamatok működéséért.
        /// </summary>
        public override void TimeAdvanced()
        {
            switch (Status)
            {
                case WorkerStatus.WanderingToWork: /*FALLTHROUGH*/
                case WorkerStatus.Wandering:
                    if(_path == null) ChooseDestination();
                    StepOneForward();
                    break;
                case WorkerStatus.Working:
                    if(_repairTimeAdvanced == ((Building)CurrentBuilding!).BuildTime)
                    {
                        ((Building)CurrentBuilding).FinishRepair();
                        _repairTimeAdvanced = 0;
                        ChooseDestination();
                    }
                    else
                    {
                        _repairTimeAdvanced++;
                    }
                    break;
            }	
        }
        
        /// <summary>
        /// Megnézi, hogy a karbantartót le lehet-e tenni a megadott pozícióra
        /// </summary>
        /// <param name="location">a pozíció</param>
        /// <param name="park">a park, amibe le akarjuk helyezni</param>
        /// <returns>A lerakás eredménye</returns>
        public override PlacementResult CanBePlacedOnLocation(GridPoint location, Park park)
        {
            if (!location.IsInField(park.Size)) return PlacementResult.OutOfField;
            return park.Facilities.Any(f => f is Road && f.Location == location)
                ? PlacementResult.Valid
                : PlacementResult.Conflict;
        }

        /// <summary>
        /// Kiválaszt egy cél épületet a karbantartónak.
        /// </summary>
        public void ChooseDestination()
        {
            CurrentBuilding = Map.Instance.GetFacility(Location);
            do
            {
                try
                {
                    Facility randomChoosenFacility = Map.Instance.GetRandomRoad(CurrentBuilding!);
                    _path = Map.Instance.GetPath(CurrentBuilding!,randomChoosenFacility);
                }
                catch (ArgumentNullException)
                {
                    _path = null;
                }
            } while (_path is null);

            Status = WorkerStatus.Wandering;
        }

        /// <summary>
        /// Megjavítja az épületet
        /// </summary>
        /// <param name="building">az épület</param>
        public void RepairBuilding(Building building)
        {
            _path = Map.Instance.GetPath(CurrentBuilding!, building);
            Status = WorkerStatus.WanderingToWork;
        }
        
        /// <summary>
        /// Akkor hívódik meg, ha a karbantartó lép egyet
        /// </summary>
        public event EventHandler<MoveEventArgs>? WorkerMoving;
    }
}

