using Model.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Serialization;

namespace Model
{
    /// <summary>
    /// A park státuszai
    /// </summary>
    public enum ParkStatus { Open, Closed }

    /// <summary>
    /// a lehelyezés lehetséges eredményei
    /// </summary>
    public enum PlacementResult { Valid, Conflict, OutOfField }

    /// <summary>
    /// A vidámpark osztály. 
    /// </summary>
    public class Park
    {

        private const int STARTING_ENTRANCE_TICKET_PRICE = 300;

        
        /// <summary>
        /// A park neve. Ez egy állapot tulajdonság.
        /// </summary>
        [XmlIgnore] 
        public State<string> NameState { get; } = new("");

        private string Name
        {
            get => NameState.Value; 
            set => NameState.Value = value;
        }

        /// <summary>
        /// Az park eredeti megnyitása óta eltelt idő.
        /// Ez egy állapot tulajdonság.
        /// </summary>
        [XmlIgnore]
        public State<TimeSpan> TimeState { get; } = new(default);

        private TimeSpan Time
        {
            get => TimeState.Value;
            set => TimeState.Value = value;
        }


        /// <summary>
        /// A TimeMode állapota.
        /// </summary>
        [XmlIgnore] 
        public State<TimeMode> TimeModeState { get; } = new(TimeMode.Normal);
        
        /// <summary>
        /// Az idő sebessége
        /// </summary>
        public TimeMode TimeMode
        {
            get => TimeModeState.Value;
            set => TimeModeState.Value = value;
        }

        /// <summary>
        /// A Budget állapota
        /// </summary>
        [XmlIgnore]
        public State<int> BudgetState { get; }
        
        /// <summary>
        /// A park pénze
        /// </summary>
        public int Budget { 
            get => BudgetState.Value; 
            set => BudgetState.Value = value;
        }


        /// <summary>
        /// A Size állapota
        /// </summary>
        [XmlIgnore]
        public State<int> SizeState { get; } = new(21);
        
        /// <summary>
        /// A park mérete
        /// </summary>
        public int Size => SizeState.Value;

        /// <summary>
        /// A ParkState állapota
        /// </summary>
        [XmlIgnore]
        public State<ParkStatus> ParkStatusState { get; }
        
        /// <summary>
        /// A park státusza
        /// </summary>
        public ParkStatus ParkStatus
        {
            get => ParkStatusState.Value;
            set => ParkStatusState.Value = value;
        }
        
        /// <summary>
        /// A parkban lévő látogatók
        /// </summary>
        public ObservableCollection<Visitor> Visitors { get; private set; }
        
        /// <summary>
        /// A parkban dolgozó karbantartók
        /// </summary>
        public ObservableCollection<MaintenanceWorker> MaintenanceWorkers { get; private set; }
        
        /// <summary>
        /// A parkban található létesítmények
        /// </summary>
        public ObservableCollection<Facility> Facilities { get; set; }


        /// <summary>
        /// A megvásárolható elemek katalógusa
        /// </summary>
        [XmlIgnore]
        public List<Purchasable> PurchasableCatalog { get; set; }

        /// <summary>
        /// Inicializál egy új Park példányt
        /// </summary>
        public Park()
        {
            PurchasableCatalog = new List<Purchasable>(new List<Purchasable>
            {
                new Road(),
                new AimShooting(),
                new Dodgem(),
                new FerrisWheel(),
                new HuntedHouse(),
                new MiniTrain(),
                new RollerCoaster(),
                new IceCreamStand(),
                new PopCornStand(),
                new Restaurant(),
                new Bush(),
                new MaintenanceWorker(),
                new Tree()
            }).OrderBy(p => p.Name).ToList();
            
            
            BudgetState = new(default);
            ParkStatusState = new(default);
            Facilities = new ObservableCollection<Facility>();
            MaintenanceWorkers = new ObservableCollection<MaintenanceWorker>();
            Visitors = new ObservableCollection<Visitor>();
            if(Facilities.Contains(Gate.Instance))
                Facilities.Add(Gate.Instance);
            Facilities.CollectionChanged += (_, _) => Map.Instance.FloydWarshall(Facilities);
        }

        /// <summary>
        /// Inicializál egy új park példányt
        /// </summary>
        /// <param name="name">a park neve</param>
        /// <param name="budget">a park budget-je</param>
        /// <param name="parkStatus">a park státusza</param>
        public Park(string name, int budget, ParkStatus parkStatus) : this()
        {
            Name = name;
            BudgetState.Value = budget;
            ParkStatus = parkStatus;
            if (!Facilities.Contains(Gate.Instance))
                Facilities.Add(Gate.Instance);
            Gate.Instance.TicketPrice = STARTING_ENTRANCE_TICKET_PRICE;
            Gate.Instance.VisitorLeaving += Gate_VisitorLeavingEventHandler;
        }

        private void Gate_VisitorLeavingEventHandler(object? sender, Visitor visitor)
        {
            Visitors.Remove(visitor);
        }

        private void BuildingVisitorEnteredHandler(object? sender, Building building)
        {
            Budget += building.TicketPrice;
        }

        private PlacementResult CanPurchasableBePlaced(Purchasable purchasable)
        {
            return CanPurchasableBePlaced(purchasable, purchasable.Location);
        }
        
        /// <summary>
        /// Megmondja, hogy le lehet-e tenni a megadott megvásárolható elemet az adott pontra
        /// </summary>
        /// <param name="purchasable">a megvásárolható elem</param>
        /// <param name="point">a pont a pályán</param>
        /// <returns>A lehelyezés eredménye</returns>
        public PlacementResult CanPurchasableBePlaced(Purchasable purchasable, GridPoint point)
        {
            return purchasable.CanBePlacedOnLocation(point, this);
        }

        /// <summary>
        /// Eltávolít egy megvásárolható elemet a pályáról
        /// </summary>
        /// <param name="purchasable"></param>
        public void RemovePurchasable(Purchasable purchasable)
        {
            switch (purchasable)
            {
                case Facility facility: Facilities.Remove(facility);
                    break;
                case MaintenanceWorker maintenanceWorker: MaintenanceWorkers.Remove(maintenanceWorker);
                    break;
            }
        }
        
        /// <summary>
        /// Megpróbálja lehelyezni adott megvásárolható elemet
        /// </summary>
        /// <param name="purchasable">a megvásárolható elem</param>
        /// <exception cref="PlacingException">ha az elemet nem sikerül lehelyezni</exception>
        public void TryPlacingPurchasable(Purchasable purchasable)
        {
            var result = CanPurchasableBePlaced(purchasable);
            if(result != PlacementResult.Valid)
            {
                switch (result)
                {
                    case PlacementResult.Conflict:
                        throw new PlacingException(result, "Conflict occured during the placement of the Purchasable.");
                    case PlacementResult.OutOfField:
                        throw new PlacingException(result, "The Purchasable tries to protrude from the map.");
                }
            }
            if (Budget >= purchasable.Price)
            {
                Budget -= purchasable.Price;
                if (Budget == 0)
                {
                    if (GameOver != null)
                    {
                        GameOver(this, true);
                    }
                }
                switch (purchasable)
                {
                    case Facility facility:
                        Facilities.Add(facility);
                        if (facility is Building building)
                        {
                            building.MoneyNeedsToBeWithdrawnFromBudget += BuildingMoneyNeedsToBeWithdrawnFromBudgetHandler;
                            building.VisitorEntered += BuildingVisitorEnteredHandler;
                        }
                        break;
                    case MaintenanceWorker worker:
                        MaintenanceWorkers.Add(worker);
                        worker.ChooseDestination();
                        break;
                }
            }
            
        }

        private void BuildingMoneyNeedsToBeWithdrawnFromBudgetHandler(object? sender, int e)
        {
            if (Budget >= e)
            {
                Budget -= e;
            }
            else
            {
                if (GameOver != null)
                {
                    GameOver(this, true);
                }
            }
        }

        /// <summary>
        /// Ez a metódus minden ticknél meghívódik. Ez felelős az automatikus folyamatok működéséért.
        /// </summary>
        public void TimeAdvanced()
        {
            if (ParkStatus == ParkStatus.Open && Map.Instance.GetBuildingsWithRoadToThis(Gate.Instance).Any(f => f.Status.IsUsable() && f is Game))
            {
                int randomNumber = new Random().Next(1, 101);
                if(randomNumber % 3 == 0 && Gate.Instance.TicketPrice < Entity.VISITOR_STARTING_MONEY_AMOUNT)
                    Visitors.Add(new Visitor(Gate.Instance.LocationOnMap,Map.Instance));
            }

            if(ParkStatus == ParkStatus.Open)
            {
                Time += TimeSpan.FromSeconds(1);
                for (int i = 0; i < Visitors.Count; i++)
                    Visitors[i].TimeAdvanced();
            }
                

            foreach (Facility facility in Facilities)
            {
                if(facility is Building && (facility as Building)!.Status is FacilityStatus.Broken)
                {
                    MaintenanceWorker? worker = Map.Instance.GetClosestMaintenanceWorker((Building)facility, MaintenanceWorkers.ToList());
                    if(worker is not null)
                    {
                        worker.RepairBuilding((Building)facility);
                        ((Building)facility).Status = FacilityStatus.WaitingForMaintenanceWorker;
                    }
                }
                facility.TimeAdvanced();
            }

            
            if(ParkStatus == ParkStatus.Open) 
            { 
                foreach (MaintenanceWorker maintenanceWorker in MaintenanceWorkers)
                    maintenanceWorker.TimeAdvanced();
            }
        }
        
        /// <summary>
        /// Megnyitja vagy bezárja a parkot, annak a státuszától függően.
        /// </summary>
        public void OpenOrClose()
        {
            ParkStatus = ParkStatus == ParkStatus.Open ? ParkStatus.Closed : ParkStatus.Open;

            switch (ParkStatus)
            {
                case ParkStatus.Open:
                    Open();
                    break;
                case ParkStatus.Closed:
                    Close();
                    break;
            }
        }

        /// <summary>
        /// Megnyitja a parkot
        /// </summary>
        private void Open()
        {
            Map.Instance.FloydWarshall(Facilities);
        }

        /// <summary>
        /// Bezárja a parkot
        /// </summary>
        private void Close()
        {
            for (int i = 0; i < Visitors.Count; i+= 0)
                Visitors.Remove(Visitors[i]);

            foreach (Facility facility in Facilities)
            {
                if(facility is Building building)
                    building.Clear();
            }
        }

        /// <summary>
        /// Akkor hívódik meg, ha a játékosnak elfogy a pénze.
        /// </summary>
        public event EventHandler<bool>? GameOver;
    }
}
