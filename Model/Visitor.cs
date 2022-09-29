using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Model.EventArguments;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Xml.Serialization;
using Model.Util;

namespace Model
{
    /// <summary>
    /// Látogató állapotai
    /// </summary>
    public enum VisitorStatus
    {
        Wandering,
        Waiting,
        Busy
    }

    /// <summary>
    /// Látogató osztály
    /// </summary>
    public class Visitor : Entity
    {
        private int _advancedTimeCounter;

        private Map? _map;

        private List<Facility>? _path;

        private int _waitingTimeCounter;

        /// <summary>
        /// Látogató üres konstruktora
        /// </summary>
        public Visitor() : base(new GridPoint(0, 0), 1, 1) { }

        /// <summary>
        /// Látogató paraméteres konstruktora
        /// </summary>
        /// <param name="point">A koordináta ahova letesszük a játékost</param>
        /// <param name="map">A vidámpark térképe útkereséshez</param>
        public Visitor(GridPoint point, Map map) : base(location: point, width: 1, height: 1)
        {
            _map = map;
            HungerState = new(0);
            MoneyState = new(VISITOR_STARTING_MONEY_AMOUNT - Gate.Instance.TicketPrice);
            HappinessState = new(100);
            ChooseDestination();
            Direction = Direction.Up;
        }

        /// <summary>
        /// Látogató boldogságának státusza
        /// </summary>
        [XmlIgnore]
        public State<int> HappinessState { get; } = new (default);
        
        /// <summary>
        /// Látogató boldogsága
        /// </summary>
        public int Happiness
        {
            get => HappinessState.Value;
            set => HappinessState.Value = value;
        }

        /// <summary>
        /// Látogató éhségének státusza
        /// </summary>
        [XmlIgnore]
        public State<int> HungerState { get; } = new (default);
        
        /// <summary>
        /// Látogató éhsége
        /// </summary>
        public int Hunger
        {
            get => HungerState.Value;
            set { HungerState.Value = value; }
        }
        
        /// <summary>
        /// Látogató pénzének státusza
        /// </summary>
        [XmlIgnore]
        public State<int> MoneyState { get; } = new (default);
        
        /// <summary>
        /// Látogató pénze
        /// </summary>
        public int Money
        {
            get => MoneyState.Value;
            set { MoneyState.Value = value; }
        }
        
        /// <summary>
        /// Látogató állapotának státusza
        /// </summary>
        [XmlIgnore]
        public State<VisitorStatus> StatusState { get; } = new(default);
        
        /// <summary>
        /// Látogató állapota
        /// </summary>
        public VisitorStatus Status 
        { 
            get => StatusState.Value; 
            set { StatusState.Value = value; } 
        }

        /// <summary>
        /// Látogató aktuális pozícióján lévő építmény
        /// </summary>
        public Facility? CurrentBuilding { get; set; }

        /// <summary>
        /// Látogató iránya
        /// </summary>
        public Direction Direction { get; set; }

        /// <summary>
        /// Látogató eseménye a helyének megváltoztatásáról
        /// </summary>
        public event EventHandler<MoveEventArgs>? PlayerMoving;

        /// <summary>
        /// Ez a metódus egyel előre lépteti a megfelelő irányba a látogatót.
        /// Először megnézi, hogy épülethez ért vagy nem, ha igen akkor megpróbálja használni,
        /// ha nem akkor megnézi merre kell tovább mennie, irányba áll és egyet előre lép.
        /// Erről jelzést is küld a nézetnek.
        /// </summary>
        private void StepOneForward()
        {
            _path!.Remove(CurrentBuilding!);
            if (_path!.Count == 0)
            {
                EnterBuilding((Building)CurrentBuilding!);
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
                        if (PlayerMoving != null)
                        {
                            PlayerMoving(this, new MoveEventArgs(new GridPoint(Location.Row, Location.Column - 1)));
                        }
                        Location = new GridPoint(Location.Row, Location.Column - 1);
                        break;
                    case Direction.Right:
                        if (PlayerMoving != null)
                        {
                            PlayerMoving(this, new MoveEventArgs(new GridPoint(Location.Row, Location.Column + 1)));
                        }
                        Location = new GridPoint(Location.Row, Location.Column + 1);
                        break;
                    case Direction.Up:
                        if (PlayerMoving != null)
                        {
                            PlayerMoving(this, new MoveEventArgs(new GridPoint(Location.Row + 1, Location.Column)));
                        }
                        Location = new GridPoint(Location.Row - 1, Location.Column);
                        break;
                    case Direction.Down:
                        if (PlayerMoving != null)
                        {
                            PlayerMoving(this, new MoveEventArgs(new GridPoint(Location.Row - 1, Location.Column)));
                        }
                        Location = new GridPoint(Location.Row + 1, Location.Column);
                        break;
                }

                CurrentBuilding = _path.First();
            }
        }
        /// <summary>
        /// Ha a látogató rendelkezik elég pénzzel, akkor a paraméterül kapott építményt megpróbálja használni.
        /// Ha nincs elegendő pénze egy új célt választ magának és elindul afelé.
        /// </summary>
        /// <param name="building">Használandó játék/étterem</param>
        private void EnterBuilding(Building building)
        {
            if(Money < building.TicketPrice)
            {
                try
                {
                    if(building is Game)
                    {
                        _path = _map!.GetPath(CurrentBuilding!, _map!.GetCheapestDestination(_map!.GetGames(building))!);
                    }
                    else
                    {
                        _path = _map!.GetPath(CurrentBuilding!, _map!.GetCheapestDestination(_map!.GetFoodBuildings(building))!);
                    }
                }
                catch (ArgumentNullException)
                {
                    _path = _map!.GetPath(CurrentBuilding!, Gate.Instance);
                }
                Status = VisitorStatus.Wandering;
            }
            else
            {
                building.TryUsing(this);
            }
        }

        /// <summary>
        /// Belép a sorba
        /// </summary>
        public void StepInQueue()
        {
            Status = VisitorStatus.Waiting;
        }

        /// <summary>
        /// Elkezdi használni a játékot ami meghívta ezt a metódust.
        /// </summary>
        /// <param name="amount">Jegy ára.</param>
        public void DeductFeeAndStartUsing(int amount)
        {   
            Money -= amount;
            Status = VisitorStatus.Busy;
        }
        /// <summary>
        /// Ez a metódus először megvizsgálja, hogy milyen építményre "van szüksége" a látogatónak.
        /// Ha éhes éttermet választ ha nem akkor pedig játékot.
        /// Sorrendben először véletlen módon hajlandóság alapján választ egy helyet, viszont ha nincsen rá elég pénze akkor megpróbál
        /// a legközelebbi helyhez menni, ha arra sincsen pénze a legolcsóbb helyet nézi ki magának. Ha pedig arra sincsen pénzze akkor elindul a kapu
        /// felé és távozik.
        /// </summary>
        private void ChooseDestination()
        {
            CurrentBuilding = _map!.GetFacility(Location);
            ObservableCollection<Building> buildings;
            if (Hunger > 75)
            {
                buildings = _map!.GetFoodBuildings((Building)CurrentBuilding!);
            }
            else
            {
                buildings = _map!.GetGames((Building)CurrentBuilding!);
            }

            if (buildings == null)
                return;

            Building? chosenDestination = _map!.ChooseBuilding(buildings);
            Building? closestDestination = ChooseClosestBuilding(buildings);
            Building? cheapestDestination = _map!.GetCheapestDestination(buildings);

            //további ellenőrzések

            if ((chosenDestination is null || chosenDestination.TicketPrice > Money) &&
               (closestDestination is null || closestDestination.TicketPrice > Money) &&
               (cheapestDestination is null || cheapestDestination.TicketPrice > Money))
            {
                _path = _map.GetPath(this.CurrentBuilding!, Gate.Instance);
            }
            else if ((chosenDestination is null || chosenDestination.TicketPrice > Money) &&
                    (closestDestination is null || closestDestination.TicketPrice > Money) &&
                    (cheapestDestination is not null && cheapestDestination.TicketPrice <= Money))
            {
                try
                {
                    _path = _map.GetPath(this.CurrentBuilding, cheapestDestination!);
                }
                catch (ArgumentNullException)
                {
                    if (closestDestination is null || closestDestination.TicketPrice < Money)
                    {
                        _path = _map.GetPath(this.CurrentBuilding, Gate.Instance);
                    }
                    else
                    {
                        _path = _map.GetPath(this.CurrentBuilding, closestDestination);
                    }
                }

            }
            else if ((chosenDestination is null || chosenDestination.TicketPrice > Money) &&
                     (closestDestination is not null && closestDestination.TicketPrice <= Money))
            {
                _path = _map.GetPath(this.CurrentBuilding, closestDestination);
            }
            else if (chosenDestination is not null && chosenDestination.TicketPrice <= Money)
            {
                try
                {
                    _path = _map.GetPath(this.CurrentBuilding!, chosenDestination);
                }
                catch (ArgumentNullException)
                {
                    if (closestDestination is null || closestDestination.TicketPrice < Money)
                    {
                        _path = _map.GetPath(this.CurrentBuilding, Gate.Instance);
                    }
                    else
                    {
                        _path = _map.GetPath(this.CurrentBuilding, closestDestination);
                    }
                }

            }

            _advancedTimeCounter = 0;
            _waitingTimeCounter = 0;
            Status = VisitorStatus.Wandering;
        }

        /// <summary>
        /// A megadott épületek közül hajlandóság alapján választ egyet (félig véletlenszerűen)
        /// </summary>
        /// <param name="buildings">Az épületek amik kötül választhat</param>
        /// <returns>A kiválasztott épület</returns>
        private Building? ChooseClosestBuilding(ObservableCollection<Building>? buildings)
        {
            try
            {
                return _map!.GetClosestBuilding((Building)CurrentBuilding!);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Ha kör közben elromlik egy játék/étterem akkor ezzel jelzik a látogató felé, hogy távoznia kell
        /// </summary>
        public void CanNotUseCurrentBuilding()
        {
            ChooseDestination();
        }

        /// <summary>
        /// A játékban eltelt egy másodpercnek felel meg, éppen az aktuális állapota alapján megnéz a látogató mit kell
        /// tennie és aszerint cselekszik.
        /// </summary>
        public override void TimeAdvanced()
        {
            switch (Status)
            {
                case VisitorStatus.Wandering:
                    StepOneForward();
                    break;
                case VisitorStatus.Busy:
                    if (_advancedTimeCounter == ((Building)CurrentBuilding!).ActivityDuration)
                    {
                        ((Building)CurrentBuilding!).StopUsing(this);
                        ChooseDestination();
                    }
                    else
                    {
                        _advancedTimeCounter++;
                        if (CurrentBuilding is FoodBuilding)
                            Hunger -= ((FoodBuilding)CurrentBuilding).HungerFactor;
                        if (Hunger < 0)
                            Hunger = 0;
                        if (CurrentBuilding is Game)
                            Happiness += ((Game)CurrentBuilding).HappinessFactor;
                        else
                            Happiness += HAPPINESS_WHEN_IN_FOODBUILDING;
                    }
                    if(Happiness > 100) Happiness = 100;
                    break;
                case VisitorStatus.Waiting:
                    if(_waitingTimeCounter >= 10)
                    {
                        Happiness -= MINUS_HAPPINESS_POINT_WHEN_WAITING;
                    }
                    _waitingTimeCounter++;
                    break;
            }
            
            Hunger += HUNGERPOINT;
            if (Hunger > 100) Hunger = 100;
            if (Hunger > 75)
                Happiness -= MINUS_HAPPINESS_POINTS_WHEN_HUNGRY;
            else if((CurrentBuilding is not Game && CurrentBuilding is not FoodBuilding) || Status is VisitorStatus.Waiting)
                Happiness -= HAPPINESSPOINT;
            Happiness += _map!.GetHappiness(CurrentBuilding!);
            if (Happiness > 100) Happiness = 100;
            if (Happiness <= 0)
            {
                _path = _map.GetPath(CurrentBuilding!, Gate.Instance);
                if(CurrentBuilding is Building || CurrentBuilding is FoodBuilding)
                    ((Building)CurrentBuilding).StopUsing(this);
                Status = VisitorStatus.Wandering;
            }
               
        }

        /// <summary>
        /// Ezt a metódust örökli és mivel a játékost nem húzzuk a képernyőre így nem hívható meg rajta ez a metódus
        /// </summary>
        /// <param name="location">Hely ahova helyezni szeretnénk</param>
        /// <param name="park">Park</param>
        /// <returns>Lehelyezhetőség státuszával tér vissza</returns>
        /// <exception cref="TargetException"></exception>
        public override PlacementResult CanBePlacedOnLocation(GridPoint location, Park park)
        {
            throw new TargetException("This method should not be called on Visitor");
        }
    }
}

