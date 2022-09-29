using Model.Util;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Xml.Serialization;

namespace Model
{
    /// <summary>
    /// Ez az absztrakt osztály implementálja az épületekre jellemző tulajdonságokat és
    /// metódusokat (sor, kihasználtság, életerő)
    /// </summary>
    public abstract class Building : Facility
    {
        /// <summary>
        /// Az épület maximális életereje
        /// </summary>
        public const int MaxHealth= 100;
        private readonly Point _gateOffset;

        private int _costTimer;

        /// <summary>
        /// Ezt a konstruktort a leszármazottaknak kell meghívni.
        /// </summary>
        /// <param name="name">az épület neve</param>
        /// <param name="location">az épület pozíciója a pályán</param>
        /// <param name="width">az épület hossza</param>
        /// <param name="height">az épület magassága</param>
        /// <param name="activityDuration">az épületben található tevékenység hossza (tickekben)</param>
        /// <param name="gateOffset">a bejárat elhelyezkedése az épületen belül ,az épület jobb felső sarka a (0,0) pont</param>
        /// <param name="generalCost">az épület általános költsége, amit tickecként levon a park pénzéből</param>
        /// <param name="visitorCost">a költség, amit látogatónként kell kifizetni a parknak</param>
        /// <param name="health">az épület életereje</param>
        protected Building(string name, GridPoint location, int width, int height, int activityDuration, Point gateOffset = default, int generalCost = 10000, int visitorCost = 5, int health = 100) : base(name, location, width, height)
        {
            _gateOffset = gateOffset;
            GeneralCost = generalCost;
            VisitorCost = visitorCost;
            ActivityDuration = activityDuration;
            UtilizationState = Users.AsState(c => c.Count);
            HealthState =new State<int> (health);
            this.Status = FacilityStatus.Building;
        }
        
        /// <summary>
        /// A épület feladatának hossza, ennyi időt tölt el a látogató az épületben
        /// </summary>
        public int ActivityDuration { get; }

        /// <summary>
        /// a minimális kihasználtság állapota százalékban
        /// </summary>
        [XmlIgnore]
        public State<int> MinimalUtilizationPercentState { get; } = new(5);
        
        /// <summary>
        /// a minimális kihasználtság százaléka, ami az épület elindításához szükséges
        /// </summary>
        public int MinimalUtilizationPercent 
        {
            get => MinimalUtilizationPercentState.Value;
            set => MinimalUtilizationPercentState.Value= value;
        }
        
        /// <summary>
        /// a minimális mennyiségő látogató, ami az épület elindításához kell.
        /// </summary>
        private int MinimumNumberOfVisitors => (int)((double)MinimalUtilizationPercent / 100 * Capacity);
        

        /// <summary>
        /// Ha az épület építés vagy javítás alatt alatt van, akkor itt számoljuk az eltelt időt.
        /// Ez egy állapot tulajdonság
        /// </summary>
        [XmlIgnore]
        public State<int> BuildAndRepairTimeState { get; } = new(0);
        
        private int BuildAndRepairTime
        {
            get => BuildAndRepairTimeState.Value; 
            set => BuildAndRepairTimeState.Value = value;
        }
        
        
        /// <summary>
        /// A maximális mennyiségű látogató, aki egyszerre használni tudja az épületet
        /// </summary>
        public int Capacity { get; set; } = 20;
        
        private int GeneralCost { get; }
        
        private int VisitorCost { get; set; }


        /// <summary>
        /// A látogatók száma az épületben. Ez egy állapot tulajdonság.
        /// </summary>
        [XmlIgnore]
        public State<int> UtilizationState { get; }
        
        private int Utilization => UtilizationState.Value;
        

        /// <summary>
        /// Az épület életereje. Ez egy állapot tulajdonság.
        /// </summary>
        [XmlIgnore]
        public State<int> HealthState { get; }
        
        private int Health
        {
            get => HealthState.Value;
            set => HealthState.Value=value; 
        }
        
        /// <summary>
        /// A tickek száma, ami az épület felépítéséhez vagy megjavításához kell
        /// </summary>
        public int BuildTime { get; set; }

        
        /// <summary>
        /// a TicketPrice állapota
        /// </summary>
        [XmlIgnore]
        public State<int> TicketPriceState { get; } = new(200);
        
        /// <summary>
        /// A jegyár, vagy az étel ára (éttermekben)
        /// </summary>
        public int TicketPrice
        {
            get => TicketPriceState.Value;
            set => TicketPriceState.Value = value;
        }
        
        private ObservableCollection<Visitor> Users { get; set; } = new();

        /// <summary>
        /// Az épület előtt álló sor
        /// </summary>
        public ObservableCollection<Visitor> Queue { get; set; } = new();
        
        private GridPoint GateLocation { get => Location + _gateOffset.ToGridPoint(); }

        /// <summary>
        /// Az útkeresés szempontjából releváns pozíció
        /// </summary>
        [XmlIgnore]
        public override GridPoint LocationOnMap
        {
            get => GateLocation;
        }

        /// <summary>
        /// Ha az épület státusza Waiting vagy Working, akkor beteszi a paraméterben
        /// átadott látogatót a sorba, egyébként visszautasítja
        /// </summary>
        /// <param name="visitor">A látogató, aki be akar jönni az épületbe</param>
        public virtual void TryUsing(Visitor visitor)
        {
            //validation of Building use
            if (this.Status != FacilityStatus.Waiting && this.Status != FacilityStatus.Working)
            {
                visitor.CanNotUseCurrentBuilding();
            }
            else if (visitor.Location == this.GateLocation)
            {
                Queue.Add(visitor);
                visitor.StepInQueue();
            }         
        }

        /// <summary>
        /// Levonja a javítás költségét és átállítja az épület státuszát Waiting-re
        /// </summary>
        public void FinishRepair()
        {
            if (Status == FacilityStatus.Repairing)
            {
                if (MoneyNeedsToBeWithdrawnFromBudget != null)
                {
                    MoneyNeedsToBeWithdrawnFromBudget(this, GeneralCost);
                }
                Status = FacilityStatus.Waiting;
                this.Health = 100;
            }   
        }

        /// <summary>
        /// Átállítja az épület státuszát Repairing-re, ha eddig az várt a karbantartóra
        /// </summary>
        public void StartRepair()
        {
            if (this.Status == FacilityStatus.WaitingForMaintenanceWorker)
            {
                this.Status = FacilityStatus.Repairing;
            }
        }
        
        private void CountTimeForGeneralCostWithdrawal() 
        {
            _costTimer++;
            if (_costTimer == 20)
            {
                if (MoneyNeedsToBeWithdrawnFromBudget != null)
                {
                    MoneyNeedsToBeWithdrawnFromBudget(this, GeneralCost);
                }
                _costTimer = 0;
            }
        }
        
        private void TryMoveFromQueueToUsers()
        {
            if (Capacity > Utilization && (Queue.Count() + Users.Count()) >= MinimumNumberOfVisitors)
            {
                int emptyPlaces = Capacity - Utilization;
                var visitorsToUse = Queue.Take(emptyPlaces).ToList();
                foreach (Visitor v in visitorsToUse)
                {
                    if (VisitorEntered != null)
                    {
                        VisitorEntered(this, this);
                    }

                    Queue.Remove(v);
                    Users.Add(v);
                    v.DeductFeeAndStartUsing(TicketPrice);
                    if (MoneyNeedsToBeWithdrawnFromBudget != null)
                    {
                        MoneyNeedsToBeWithdrawnFromBudget(this, VisitorCost);
                    }
                }
                Status = FacilityStatus.Working;
            }

        }
        
        private void ConstructBuilding() 
        {
            BuildAndRepairTime++;
            if (BuildTime == BuildAndRepairTime)
            {
                BuildAndRepairTime = 0;
                Status = FacilityStatus.Waiting;
            }
        }
        
        /// <summary>
        /// Kilépteti a paraméterként átadott létogatót az épületből. Ha az épületnek elfogyott az életereje, akkor elromlik
        /// Ha az látogató a sorban van, onnan is eltűnik.
        /// </summary>
        /// <param name="visitor">a látogató akit ki akarunk léptetni</param>
        public void StopUsing(Visitor visitor)
        {
            if (Users.Contains(visitor))
            {
                Users.Remove(visitor);
                if (Users.Count() == 0)
                {
                    Status = FacilityStatus.Waiting;
                }
                if (Health <= 0)
                {
                    this.Status = FacilityStatus.Broken;
                }
                else
                {
                    Health -= 1;
                }
            }
            if (Queue.Contains(visitor))
            {
                Queue.Remove(visitor);
            }
        }
        
        private void SendOutVisitors() 
        {
            foreach (Visitor v in Queue)
            {
                v.CanNotUseCurrentBuilding();
            }
            Queue.Clear();
            foreach (Visitor v in Users)
            {
                v.CanNotUseCurrentBuilding();
            }
            Users.Clear();
        }
        
        
        /// <summary>
        /// Az összes épületet kiküldi az épületből, illetve az előtte álló sorból.
        /// </summary>
        public void Clear()
        {
            Users.Clear();
            Queue.Clear();
            if(Status.IsUsable()) Status = FacilityStatus.Waiting;
        }

        /// <summary>
        /// Ez a metódus minden ticknél meghívódik. Ez felelős az automatikus folyamatok működéséért.
        /// </summary>
        public override void TimeAdvanced()
        {
            switch (this.Status)
            {
                case FacilityStatus.Waiting or FacilityStatus.Working:
                    CountTimeForGeneralCostWithdrawal();
                    TryMoveFromQueueToUsers();
                    break;
                case FacilityStatus.Building:
                    ConstructBuilding();
                    break;
                case FacilityStatus.Broken:
                    SendOutVisitors();
                    break;
                case FacilityStatus.WaitingForMaintenanceWorker:
                    SendOutVisitors();
                    break;
            }
        }
        
        /// <summary>
        /// Akkor hívódik meg, ha pénzt kell levonni a parktól
        /// </summary>
        public event EventHandler<int>? MoneyNeedsToBeWithdrawnFromBudget;
        
        /// <summary>
        /// Akkor hívódik meg, ha egy visitor belép az épületbe
        /// </summary>
        public event EventHandler<Building>? VisitorEntered;
    }
}
