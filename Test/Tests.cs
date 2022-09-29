using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model;
using Model.Util;
using Persistence;
using ViewModel.Util;

namespace Test;

[TestClass]
public class Tests
{
    private GameModel _model;

    public Tests()
    {
        _model = new GameModel(new XmlPersistence());
        _model.NewGame("test park");
        //_model.OpenGameAsync("../../../test.rtx");
    }

    [TestInitialize]
    public void InitializePark()
    {
        _model.OpenGameAsync("../../../test.rtx");

    }

    [TestMethod]
    public void StateMap()
    {
        var state = new State<int>(10);
        var state2 = state.Select(i => i + 3);
        Assert.AreEqual(10, state.Value);
        Assert.AreEqual(13, state2.Value);

        state.Value = 20;
        Assert.AreEqual(23, state2.Value);
    }

    [TestMethod]
    public void StateMapTwoWay()
    {
        var state = new State<int>(2);
        var state2 = state.SelectTwoWay(i => i.ToString(), int.Parse);
        
        Assert.AreEqual("2", state2.Value);
        
        state.Value = 10;
        Assert.AreEqual("10", state2.Value);

        state2.Value = "100";
        Assert.AreEqual(100, state.Value);
    }

    [TestMethod]
    public void StateInfluence()
    {
        var state = new State<int>(10);
        var state2 = new State<int>(30);
        
        state.Subscribe(state2);
        Assert.AreEqual(10, state2.Value);

        state.Value = 100;
        Assert.AreEqual(100, state2.Value);
    }

    [TestMethod]
    public void StateGetConstantStateMember()
    {
        var state = new State<A>(new A(new State<int>(10)));
        var state2 = state.SelectMany(a => a.B);
        Assert.AreEqual(10, state2.Value);

        state.Value.B.Value = 100;
        Assert.AreEqual(100, state2.Value);

        state.Value = new A(new State<int>(15));
        Assert.AreEqual(15, state2.Value);

        state.Value.B.Value = 30;
        Assert.AreEqual(30, state2.Value);
    }

    [TestMethod]
    public void ObservableCollectionAsState()
    {
        var oc = new ObservableCollection<int>();
        var sumState = oc.AsState(c => c.Sum());
        Assert.AreEqual(0, sumState.Value);
        
        oc.Add(10);
        Assert.AreEqual(10, sumState.Value);
        
        oc.Add(30);
        Assert.AreEqual(40, sumState.Value);

        oc.Remove(10);
        Assert.AreEqual(30, sumState.Value);
    }

    [TestMethod]
    public void StateAsDelegateCommandCanExecute()
    {
        var i = 0;
        var state = new State<bool>(true);
        var dc = new DelegateCommand(state, _ => i++);
        Assert.AreEqual(0, i);
        
        dc.Execute(null);
        Assert.AreEqual(1,i);

        dc.CanExecuteChanged += (_, _) => i++;
        state.Value = false;
        Assert.AreEqual(2, i);
        
        dc.Execute(null);
        Assert.AreEqual(2, i);

    }

    [TestMethod]
    public void ObservableCollectionExtensionInfluence()
    {
        var oc = new ObservableCollection<string>();
        oc.Add("alma");

        var oc2 = new ObservableCollection<string>();
        oc.Subscribe(oc2);
        Assert.AreEqual(1, oc2.Count);
        Assert.AreEqual("alma", oc2[0]);
        
        oc.Add("körte");
        Assert.AreEqual(2, oc2.Count);
        Assert.AreEqual("körte", oc2[1]);

        oc.Remove("alma");
        Assert.AreEqual(1, oc2.Count);
        Assert.AreEqual("körte", oc[0]);
    }

    [TestMethod]
    public void StateGetConstantCollectionMember()
    {
        var aState = new State<A>(new A(new State<int>(10)));
        var oc = aState.SelectMany(a => a.ObservableCollection);
        
        aState.Value.ObservableCollection.Add("alma");
        Assert.AreEqual(1, oc.Count);
        Assert.AreEqual("alma", oc[0]);

        aState.Value = new A(new State<int>(34));
        Assert.AreEqual(0, oc.Count);
        
        aState.Value.ObservableCollection.Add("körte");
        Assert.AreEqual(1, oc.Count);
        Assert.AreEqual("körte", oc[0]);
    }

    [TestMethod]
    public void ObservableCollectionMap()
    {
        var oc = new ObservableCollection<string>();
        oc.Add("1");
        var oc2 = oc.Map(int.Parse);
        Assert.AreEqual(1, oc2.Count);
        Assert.AreEqual(1, oc2[0]);
        
        oc.Add("20");
        Assert.AreEqual(2, oc2.Count);
        Assert.AreEqual(20, oc2[1]);

        oc.Remove("1");
        Assert.AreEqual(1, oc2.Count);
        Assert.AreEqual(20, oc2[0]);
    }

    [TestMethod]
    public void ObservableCollectionFold()
    {
        var oc = new ObservableCollection<State<int>>();
        var sumState = oc.Aggregate(0, (i, i2) => i + i2);
        Assert.AreEqual(0, sumState.Value);

        var state = new State<int>(10);
        oc.Add(state);
        Assert.AreEqual(10, sumState.Value);
        
        oc.Add(new State<int>(20));
        Assert.AreEqual(30, sumState.Value);

        oc.Remove(state);
        Assert.AreEqual(20, sumState.Value);
    }

    [TestMethod]
    public void StateCombine()
    {
        var s1 = new State<int>(10);
        var s2 = new State<string>("alma");
        var s3 = s1.Combine(s2, (i, s) => $"{i} {s}");
        Assert.AreEqual("10 alma", s3.Value);

        s1.Value = 20;
        Assert.AreEqual("20 alma", s3.Value);

        s2.Value = "körte";
        Assert.AreEqual("20 körte", s3.Value);
    }

    [TestMethod]
    public void ObservableCollectionMerge()
    {
        var o1 = new ObservableCollection<int>(new List<int> {1, 2, 3, 4});
        var o2 = new ObservableCollection<int>(new List<int> {10, 20, 30, 40});
        var o3 = o1.Merge(o2);
        Assert.AreEqual(8, o3.Count);
        Assert.IsTrue(o3.Contains(30));
        Assert.IsTrue(o3.Contains(3));
        
        o1.Add(100);
        Assert.AreEqual(9, o3.Count);
        Assert.IsTrue(o3.Contains(100));
        
        o2.Add(250);
        Assert.AreEqual(10, o3.Count);
        Assert.IsTrue(o3.Contains(250));

        o1.Remove(2);
        Assert.AreEqual(9, o3.Count);
        Assert.IsFalse(o3.Contains(2));

        o2.Remove(250);
        Assert.AreEqual(8, o3.Count);
        Assert.IsFalse(o3.Contains(250));
    }
    
    [TestMethod]
    public void GridPointIsInField()
    {
        var gp = new GridPoint(1, 3);
        Assert.IsTrue(gp.IsInField(4));
        Assert.IsFalse(gp.IsInField(3));
    }

    [TestMethod]
    public void MaintenanceWorkerCanBePlacedOnLocation()
    {
        var park = new Park();
        park.Budget = 30000;
        park.TryPlacingPurchasable(new Road(new GridPoint(0,0)));
        var mw = new MaintenanceWorker(new GridPoint(0, 0));
        Assert.AreEqual(PlacementResult.Valid,mw.CanBePlacedOnLocation(new GridPoint(0,0), park));
        Assert.AreEqual(PlacementResult.OutOfField, mw.CanBePlacedOnLocation(new GridPoint(100,100), park));
        Assert.AreEqual(PlacementResult.Conflict, mw.CanBePlacedOnLocation(new GridPoint(1, 1), park));
    }

    [TestMethod]
    public void BuildingTimeAdvancePlaceStatusBuilding() 
    {
        var aimshooting = new AimShooting();
        aimshooting.Status = FacilityStatus.Building;
        for (int i = 0; i < 10; i++)
        {           
            Assert.AreEqual(FacilityStatus.Building, aimshooting.Status);
            aimshooting.TimeAdvanced();
        }
        Assert.AreEqual(FacilityStatus.Waiting, aimshooting.Status);

    }

    [TestMethod]
    public void BuildingTimeAdvancePlaceStatusWorking()
    {
        var aimshooting = new AimShooting();
        aimshooting.MinimalUtilizationPercent = 0;
        aimshooting.Status = FacilityStatus.Waiting;
        var visitor1 = new Visitor();
        aimshooting.TryUsing(visitor1);
        aimshooting.TimeAdvanced();
        Assert.AreEqual(FacilityStatus.Working, aimshooting.Status);
    }

    [TestMethod]
    public void BuildingMinimalUtilizationPercent()
    {
        var aimshooting = new AimShooting();
        aimshooting.MinimalUtilizationPercent = 10;
        aimshooting.Status = FacilityStatus.Waiting;
        var visitor1 = new Visitor();
        aimshooting.TryUsing(visitor1);
        aimshooting.TimeAdvanced();
        Assert.AreEqual(FacilityStatus.Waiting, aimshooting.Status);
        var visitor2 = new Visitor();
        aimshooting.TryUsing(visitor2);
        aimshooting.TimeAdvanced();
        Assert.AreEqual(FacilityStatus.Working, aimshooting.Status);
    }

    [TestMethod]
    public void TestMapFloydWarshall()
    {
        Map.Instance.Flush();

        Assert.AreEqual(0,Map.Instance.Distance.Count);
        Assert.AreEqual(0,Map.Instance.Pi.Count);

        var facilities = BuildMiniMap();

        Map.Instance.FloydWarshall(facilities);

        Assert.AreEqual(facilities.Count,Map.Instance.Distance.Count);
        foreach (KeyValuePair<Facility, Dictionary<Facility, long>> pair in Map.Instance.Distance)
        {
            Assert.AreEqual(facilities.Count,pair.Value.Count);
        }

        //mindegyik között egy a távolság
        for(int i = 0; i < facilities.Count - 1; i++)
            Assert.AreEqual(1,Map.Instance.Distance[facilities[i]][facilities[i+1]]);
    }

    [TestMethod]
    public void TestMapGetClosestMaintenanceWorker()
    {
        Map.Instance.Flush();

        var workers = new List<MaintenanceWorker>();

        Map.Instance.FloydWarshall(_model.Park.Facilities);
        
        workers.Add(new MaintenanceWorker(new GridPoint(16,10)));
        workers.Add(new MaintenanceWorker(new GridPoint(18,10)));
        workers.Add(new MaintenanceWorker(new GridPoint(Gate.Instance.LocationOnMap.Row,Gate.Instance.LocationOnMap.Column)));

        var worker = Map.Instance.GetClosestMaintenanceWorker(_model.Park.Facilities.Where(f => f is Game).FirstOrDefault() as Building, workers);

        Assert.IsNotNull(worker);
        Assert.AreEqual(new GridPoint(16, 10), worker.Location); // a legközelebbi munkást hívja meg

        workers.First().Status = WorkerStatus.WanderingToWork;

        worker = Map.Instance.GetClosestMaintenanceWorker(_model.Park.Facilities.Where(f => f is Game).FirstOrDefault() as Building, workers);

        Assert.IsNotNull(worker);
        Assert.AreNotEqual(new GridPoint(16, 10), worker.Location); // nem a legközelebbi munkás jön
        Assert.AreEqual(new GridPoint(18, 10), worker.Location); // a mivel a legközelebbi munkás elfoglalt így a második legközelebbi jön

        workers.RemoveRange(1,2);

        worker = Map.Instance.GetClosestMaintenanceWorker(_model.Park.Facilities.Where(f => f is Game).FirstOrDefault() as Building, workers);

        Assert.IsNull(worker); // mivel az első és egyben egyetlen munkás elfoglalt így nem jön munkás az épülethez
    }

    [TestMethod]
    public void TestFilterFacilitiesWithParameter()
    {
        Map.Instance.Flush();

        var facilities = new ObservableCollection<Facility>();
        facilities.Add((new FerrisWheel(new GridPoint(0,0))));
        facilities.Add((new AimShooting(new GridPoint(1, 1))));
        facilities.Add((new Dodgem(new GridPoint(2, 2))));
        facilities.Add((new GiantSwing(new GridPoint(3, 3))));
        facilities.Add((new HuntedHouse(new GridPoint(4, 4))));
        facilities.Add((new MiniTrain(new GridPoint(5, 5))));
        facilities.Add((new RollerCoaster(new GridPoint(6, 6))));

        facilities.Add((new IceCreamStand(new GridPoint(7, 7))));
        facilities.Add((new PopCornStand(new GridPoint(8, 8))));
        facilities.Add((new Restaurant(new GridPoint(9, 9))));

        Map.Instance.FloydWarshall(facilities);

        Assert.AreEqual(7,Map.Instance.GetGames(null!).Count);
        Assert.AreEqual(3,Map.Instance.GetFoodBuildings(null!).Count);
    }

    [TestMethod]
    public void TestGetClosestBuilding()
    {
        var facilities = new ObservableCollection<Facility>();
        var ferrisWheel = new FerrisWheel(new GridPoint(10, 10));
        var dodgem = new Dodgem(new GridPoint(17, 11));

        ferrisWheel.Status = FacilityStatus.Working;
        dodgem.Status = FacilityStatus.Working;

        facilities.Add(Gate.Instance);
        facilities.Add(new Road(new GridPoint(19,10)));
        facilities.Add(new Road(new GridPoint(18, 10)));
        facilities.Add(new Road(new GridPoint(17, 10)));
        facilities.Add(dodgem);
        facilities.Add(new Road(new GridPoint(16,10)));
        facilities.Add(new Road(new GridPoint(15, 10)));
        facilities.Add(new Road(new GridPoint(14, 10)));
        facilities.Add(new Road(new GridPoint(13, 10)));
        facilities.Add(new Road(new GridPoint(12, 10)));
        facilities.Add(ferrisWheel);

        Map.Instance.FloydWarshall(facilities);

        Assert.AreEqual(dodgem,Map.Instance.GetClosestBuilding(Gate.Instance));
        Assert.AreEqual(ferrisWheel, Map.Instance.GetClosestBuilding(dodgem));
    }

    [TestMethod]
    public void TestMapGetCheapestDestination()
    {

        var buildings = new ObservableCollection<Building>
        {
            new FerrisWheel(new GridPoint(0, 0)),
            new HuntedHouse(new GridPoint(1, 1)),
            new Dodgem(new GridPoint(2,2))
        };

        buildings[0].Status = buildings[1].Status = buildings[2].Status = FacilityStatus.Working;
        buildings[0].TicketPrice = 1000;
        buildings[1].TicketPrice = 2000;
        buildings[2].TicketPrice = 3000;

        Assert.AreEqual(buildings[0],Map.Instance.GetCheapestDestination(buildings)); 

        buildings[0].TicketPrice = 3000;

        Assert.AreEqual(buildings[1],Map.Instance.GetCheapestDestination(buildings));

        buildings.Clear();

        //éttermekre, standokra is ugyanúgy működik
        buildings.Add(new IceCreamStand(new GridPoint(0,0)));
        buildings.Add(new PopCornStand(new GridPoint(1, 1)));
        buildings.Add(new Restaurant(new GridPoint(2, 2)));

        buildings[0].Status = buildings[1].Status = buildings[2].Status = FacilityStatus.Working;

        buildings[0].TicketPrice = 1000;
        buildings[1].TicketPrice = 2000;
        buildings[2].TicketPrice = 3000;

        Assert.AreEqual(buildings[0], Map.Instance.GetCheapestDestination(buildings));

        buildings[0].TicketPrice = 3000;

        Assert.AreEqual(buildings[1], Map.Instance.GetCheapestDestination(buildings));
    }

    [TestMethod]
    public void TestMapGetPath()
    {
        var facilities = BuildMiniMap();
        Map.Instance.FloydWarshall(facilities);

        var wheel = facilities.Where(f => f is FerrisWheel).First();

        var path = Map.Instance.GetPath(Gate.Instance, wheel);

        Assert.IsNotNull(path); // kell lennie útnak
        Assert.AreEqual(facilities.Count, path.Count);

        var restaurant = new Restaurant(new GridPoint(0, 0));
        facilities.Add(restaurant);

        Map.Instance.FloydWarshall(facilities);

        try
        {
            path = Map.Instance.GetPath(Gate.Instance, restaurant);
        }
        catch (ArgumentNullException)
        {
            path = null;
        }

        Assert.IsNull(path); // nem szabad útnak lennie a két épület között

        try
        {
            path = Map.Instance.GetPath(wheel, restaurant);
        }
        catch (ArgumentNullException)
        {
            path = null;
        }

        Assert.IsNull(path); // itt sem szabad útnak lennie a két épület között
    }

    [TestMethod]
    public void TestMapGetHappiness()
    {
        var facilities = BuildMiniMap();

        facilities.Add(new Bush(new GridPoint(17,9)));
        facilities.Add(new Bush(new GridPoint(17,11)));

        var road = facilities.Where(f => f is Road && f.Location.Equals(new GridPoint(17, 10))).First();

        Map.Instance.FloydWarshall(facilities);

        Assert.AreEqual(2, Map.Instance.GetHappiness(road)); // 1 széles és 1 magas épületre vizsgáljuk van-e mellette növény

        facilities.Add(new Bush(new GridPoint(13, 9)));
        facilities.Add(new Bush(new GridPoint(14, 9)));
        facilities.Add(new Bush(new GridPoint(12, 10)));
        facilities.Add(new Bush(new GridPoint(12, 11)));
        facilities.Add(new Bush(new GridPoint(13, 12)));
        facilities.Add(new Bush(new GridPoint(14, 12)));
        facilities.Add(new Bush(new GridPoint(15, 11)));

        var wheel = facilities.Where(f => f is FerrisWheel).First();

        Map.Instance.FloydWarshall(facilities);

        Assert.AreEqual(7,Map.Instance.GetHappiness(wheel)); // több széles és több magas épületre vizsgáljuk van-e mellette növény
    }

    [TestMethod]
    public void TestMapGetFacility()
    {
        var facilities = BuildMiniMap();

        Map.Instance.FloydWarshall(facilities);

        var wheel = facilities.Where(f => f is FerrisWheel).First();

        Assert.AreEqual(wheel,Map.Instance.GetFacility(new GridPoint(13,10))); // a lehelyezés pontja, így is meg kell találnia
        Assert.AreEqual(wheel,Map.Instance.GetFacility(new GridPoint(14,10))); // a kapu bejárata, így is meg kell találnia hiszen nem fedhet át más épülettel
    }

    [TestMethod]
    public void TestMapGetBuildingsWithRoadToThis()
    {
        var facilities = BuildMiniMap();

        Map.Instance.FloydWarshall(facilities);

        var wheel = facilities.Where(f => f is FerrisWheel).First();

        Assert.AreEqual(1, Map.Instance.GetBuildingsWithRoadToThis(Gate.Instance).Count);
        Assert.AreEqual(1, Map.Instance.GetBuildingsWithRoadToThis(wheel).Count);

        var restaurant = new Restaurant(new GridPoint(18, 13));

        facilities.Add(restaurant);

        Map.Instance.FloydWarshall(facilities);

        Assert.AreEqual(1, Map.Instance.GetBuildingsWithRoadToThis(Gate.Instance).Count);
        Assert.AreEqual(1, Map.Instance.GetBuildingsWithRoadToThis(wheel).Count);
        Assert.AreEqual(0,Map.Instance.GetBuildingsWithRoadToThis(restaurant).Count);

        facilities.Add(new Road(new GridPoint(18,11)));
        facilities.Add(new Road(new GridPoint(18,12)));

        Map.Instance.FloydWarshall(facilities);

        Assert.AreEqual(2,Map.Instance.GetBuildingsWithRoadToThis(Gate.Instance).Count);
        Assert.AreEqual(2,Map.Instance.GetBuildingsWithRoadToThis(wheel).Count);
        Assert.AreEqual(2,Map.Instance.GetBuildingsWithRoadToThis(restaurant).Count);
    }

    [TestMethod]
    public void TestMapGetRandomRoad()
    {
        var facilities = BuildMiniMap();

        Map.Instance.FloydWarshall(facilities);

        var currentRoad = facilities.Where(f => f is Road).First();

        var nextRoad = Map.Instance.GetRandomRoad(currentRoad);

        Assert.IsTrue(nextRoad is Road); // amit választ az út
        Assert.AreNotEqual(currentRoad,nextRoad); //nem ugyanazt az utat választja amin áll

        var currentBuilding = facilities.Where(f => f is FerrisWheel).First();

        nextRoad = Map.Instance.GetRandomRoad(currentBuilding);

        Assert.IsTrue(nextRoad is Road); // épületből indult, de utat választott
        Assert.AreNotEqual(currentBuilding,nextRoad);
    }

    [TestMethod]
    public void TestPlayerMovingToGame()
    {
        Map.Instance.FloydWarshall(_model.Park.Facilities);

        Visitor v = new Visitor(Gate.Instance.LocationOnMap, Map.Instance);

        Assert.AreEqual(VisitorStatus.Wandering, v.Status);
        Assert.AreEqual(new GridPoint(20,10),v.Location);
        Assert.AreEqual(Direction.Up,v.Direction);

        v.TimeAdvanced();

        Assert.AreEqual(new GridPoint(19,10),v.Location);
        Assert.AreEqual(Direction.Up, v.Direction);

        for (int i = 0; i < 3; i++)
        {
            v.TimeAdvanced();
        }

        v.TimeAdvanced();

        Assert.AreEqual(new GridPoint(15,10),v.Location);

        v.TimeAdvanced();

        Assert.AreEqual(Direction.Left,v.Direction);

        for (int i = 0; i < 6; i++)
        {
            v.TimeAdvanced();
        }

        Assert.AreEqual(VisitorStatus.Waiting,v.Status);
    }

    [TestMethod]
    public void TestVisitorPropertiesChanging()
    {
        Map.Instance.FloydWarshall(_model.Park.Facilities);

        Visitor v1 = new Visitor(Gate.Instance.LocationOnMap, Map.Instance);
        Visitor v2 = new Visitor(Gate.Instance.LocationOnMap, Map.Instance);

        _model.Park.Visitors.Add(v1);
        _model.Park.Visitors.Add(v2);

        int previousHunger = v1.Hunger;
        int previousHappiness = v1.Happiness;

        int i = 0;
        do
        {
            v1.TimeAdvanced();
            Assert.AreNotEqual(previousHunger, v1.Hunger);
            Assert.AreEqual(previousHunger + 1, v1.Hunger);
            Assert.AreNotEqual(previousHappiness, v1.Happiness);
            Assert.AreEqual(previousHappiness - 1, v1.Happiness);
            previousHunger = v1.Hunger;
            previousHappiness = v1.Happiness;
            i++;
        } while (i < 8);
        v1.TimeAdvanced(); v1.TimeAdvanced();
        Assert.AreEqual(VisitorStatus.Waiting, v1.Status);

        for(i = 0; i < 11; i++)
            v1.TimeAdvanced();

        previousHappiness = v1.Happiness;
        v1.TimeAdvanced();

        Assert.AreNotEqual(previousHappiness, v1.Happiness);
        Assert.AreEqual(previousHappiness - 3, v1.Happiness);

        for( i = 0; i < 10; i++)
            v2.TimeAdvanced();

        _model.Park.ParkStatus = ParkStatus.Open;

        _model.TimeAdvanced();

        previousHappiness = v1.Happiness;

        v1.TimeAdvanced(); v2.TimeAdvanced();

        Assert.AreEqual(VisitorStatus.Busy, v1.Status);
        Assert.AreEqual(VisitorStatus.Busy, v2.Status);

        Assert.AreNotEqual(previousHappiness, v1.Happiness);
        Assert.AreEqual(previousHappiness + (v1.CurrentBuilding as Game).HappinessFactor, v1.Happiness);
    }


    [TestMethod]
    public void TestHungerChanging()
    {
        _model.Park.TryPlacingPurchasable(new PopCornStand(new GridPoint(17,11)));

        Map.Instance.FloydWarshall(_model.Park.Facilities);

        Visitor v = new Visitor(Gate.Instance.LocationOnMap, Map.Instance);

        _model.Park.Visitors.Add(v);

        _model.Park.ParkStatus = ParkStatus.Open;

        for(int i = 0;i < 30; i++)
            _model.TimeAdvanced();

        v.Hunger = 80;

        //ennyi lépésből ér el a kajáldához
        for (int i = 0; i < 9; i++)
        {
            int previousHappines = v.Happiness;
            _model.TimeAdvanced();
            Assert.AreNotEqual(previousHappines, v.Happiness);
            //-3 pont amikor éhes annyival csökken a kedve
            Assert.AreEqual(previousHappines - 3, v.Happiness);
        }
            

        int previousHunger = v.Hunger;

        v.TimeAdvanced();

        Assert.IsTrue(v.CurrentBuilding is FoodBuilding);
        Assert.AreNotEqual(previousHunger, v.Hunger);
        //a plusz 1 az általános hunger pont ami mindig hozzáadódik minden egyes másodpercben függetlenül attól ,hogy mit csinál
        Assert.AreEqual(previousHunger - (v.CurrentBuilding as FoodBuilding).HungerFactor + 1, v.Hunger);
    }

    [TestMethod]
    public void TestMaintenanceWorkersMoves()
    {
        Map.Instance.FloydWarshall(_model.Park.Facilities);

        var worker = new MaintenanceWorker(Gate.Instance.LocationOnMap);

        _model.Park.MaintenanceWorkers.Add(worker);

        var prevLocation = worker.Location;

        _model.Park.ParkStatus = ParkStatus.Open;

        _model.TimeAdvanced();

        Assert.AreNotEqual(prevLocation, worker.Location);
        Assert.AreEqual(WorkerStatus.Wandering, worker.Status);

        var ferris = _model.Park.Facilities.Where(f => f is Game).First() as Game;

        ferris.Status = FacilityStatus.Broken;

        _model.TimeAdvanced();

        Assert.AreEqual(WorkerStatus.WanderingToWork, worker.Status);
        for (int i = 0; i < 10; i++)
        {
            _model.TimeAdvanced();
        }
            

        Assert.AreEqual(WorkerStatus.Working, worker.Status);

        for(int i = 0; i < 20; i++)
            _model.TimeAdvanced();

        Assert.AreEqual(100, ferris.HealthState.Value);

        Assert.AreEqual(WorkerStatus.Wandering, worker.Status);
    }

    [TestMethod]
    [ExpectedException(typeof(PlacingException))]
    public void TestFacilityCanNotBePlacedBecauseOfConflict()
    {
        Map.Instance.FloydWarshall(_model.Park.Facilities);

        FerrisWheel wheel = new FerrisWheel(new GridPoint(19, 10));
        _model.Park.TryPlacingPurchasable(wheel);
    }

    [TestMethod]
    [ExpectedException(typeof(PlacingException))]
    public void TestFacilityCanNotBePlacedBecauseOfOutOfField()
    {
        Map.Instance.FloydWarshall(_model.Park.Facilities);

        FerrisWheel wheel = new FerrisWheel(new GridPoint(20, 20));
        _model.Park.TryPlacingPurchasable(wheel);
    }

    [TestMethod]
    public void TestFacilityCanBePlaced()
    {
        Map.Instance.FloydWarshall(_model.Park.Facilities);

        FerrisWheel wheel = new FerrisWheel(new GridPoint(1, 1));
        _model.Park.TryPlacingPurchasable(wheel);
    }

    [TestMethod]
    public void TestPlayerCantUseGame()
    {
        Map.Instance.FloydWarshall(_model.Park.Facilities);

        _model.Park.ParkStatus = ParkStatus.Open;

        Visitor v = new Visitor(Gate.Instance.LocationOnMap, Map.Instance);

        _model.Park.Visitors.Add(v);

        var wheel = _model.Park.Facilities.Where(f => f is Game).First() as Game;

        wheel.TicketPrice = 1000000;

        for(int i = 0; i < 9; i++)
            _model.TimeAdvanced();

        Assert.AreEqual(wheel, v.CurrentBuilding);

        _model.TimeAdvanced();
        _model.TimeAdvanced();

        Assert.AreNotEqual(wheel, v.CurrentBuilding);

        for(int i = 0; i < 9; i++)
            _model.TimeAdvanced();

        Assert.AreEqual(Gate.Instance, v.CurrentBuilding);
    }

    [TestMethod]
    public void BuildingBrokesDown()
    {
        Map.Instance.FloydWarshall(_model.Park.Facilities);

        var wheel = _model.Park.Facilities.Where(f => f is Game).First() as Game;

        _model.Park.ParkStatus = ParkStatus.Open;

        for (int i = 0; i < 20; i++)
        {
            Visitor v = new Visitor(wheel.LocationOnMap, Map.Instance);
            _model.Park.Visitors.Add(v);
            wheel.TryUsing(v);
        }

        _model.TimeAdvanced();

        for(int i = 0; i < 20; i++)
            Assert.AreEqual(VisitorStatus.Busy, _model.Park.Visitors[i].Status);

        wheel.Status = FacilityStatus.Broken;

        _model.TimeAdvanced();

        for(int i = 0; i < 20; i++)
            Assert.AreEqual(VisitorStatus.Wandering, _model.Park.Visitors[i].Status);
    }

    [Ignore]
    private ObservableCollection<Facility> BuildMiniMap()
    {
        ObservableCollection<Facility> facilities = new ObservableCollection<Facility>();

        facilities.Add(Gate.Instance);
        facilities.Add(new Road(new GridPoint(19,10)));
        facilities.Add(new Road(new GridPoint(18, 10)));
        facilities.Add(new Road(new GridPoint(17, 10)));
        facilities.Add(new Road(new GridPoint(16, 10)));
        facilities.Add(new Road(new GridPoint(15, 10)));
        facilities.Add(new FerrisWheel(new GridPoint(13,10)));
        

        return facilities;
    }

    


    private class A
    {
        public A(State<int> b)
        {
            B = b;
        }

        public State<int> B { get; }
        public ObservableCollection<string> ObservableCollection { get; } = new();
    }
}