using Model.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// SINGLETON
    /// Térkép osztály a parkhoz
    /// </summary>
    public class Map
    {
        private static Map? _instance;
        private Dictionary<Facility, Dictionary<Facility, long>> _distance;

        private Dictionary<Facility, Dictionary<Facility, Facility?>> _pi;

        /// <summary>
        /// Az összes épület
        /// </summary>
        public ObservableCollection<Facility>? Facilities { get; private set; }
        
        /// <summary>
        /// Távolság gyűjtemény.
        /// Megmondja melyik épülettől egy másik épület milyen messze van.
        /// Főátlóiban csupa 0 van.
        /// </summary>
        public Dictionary<Facility, Dictionary<Facility,long>> Distance { get => _distance; }

        /// <summary>
        /// Szülő gyűjtemény.
        /// Megmondja, hogy két épólet között milyen másik épületeken kell keresztül menni.
        /// Rekurzívan vag iteratívan előállítható belőle az útvonal.
        /// </summary>
        public Dictionary<Facility, Dictionary<Facility, Facility>> Pi { get => _pi; }


        /// <summary>
        /// Map példány
        /// </summary>
        public static Map Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Map();

                return _instance;
            }
        }

        /// <summary>
        /// Konstruktor
        /// </summary>
        private Map()
        {
            _distance = new Dictionary<Facility, Dictionary<Facility, long>>();
            _pi = new Dictionary<Facility, Dictionary<Facility, Facility?>>();
        }

        /// <summary>
        /// Adatok törlése
        /// </summary>
        public void Flush()
        {
            _distance.Clear();
            _pi.Clear();
        }

        /// <summary>
        /// A kapott épületekből felépít egy kezdeti gráfot, megnézi ki kinek a szomszédja
        /// </summary>
        /// <param name="facilities">Építmények listája</param>
        private void MakeGraph(ObservableCollection<Facility> facilities)
        {
            Flush();
            Facilities = facilities;
            foreach (Facility facilityOne in facilities)
            {
                _distance.Add(facilityOne, new Dictionary<Facility, long>());
                List<GridPoint> neighbours = new List<GridPoint>();
                neighbours.Add(new GridPoint(facilityOne.LocationOnMap.Row + 1, facilityOne.LocationOnMap.Column + 0)); // alsó szomszéd
                neighbours.Add(new GridPoint(facilityOne.LocationOnMap.Row - 1, facilityOne.LocationOnMap.Column + 0)); // felső szomszéd
                neighbours.Add(new GridPoint(facilityOne.LocationOnMap.Row + 0, facilityOne.LocationOnMap.Column + 1)); // jobb szomszéd
                neighbours.Add(new GridPoint(facilityOne.LocationOnMap.Row + 0, facilityOne.LocationOnMap.Column - 1)); // bal szomszéd

                foreach (Facility facilityTwo in facilities)
                {
                    if (facilityOne.Equals(facilityTwo))
                    {
                        _distance[facilityOne].Add(facilityTwo, 0);
                    }
                    else if (neighbours.Contains(facilityTwo.LocationOnMap))
                    {
                        _distance[facilityOne].Add(facilityTwo, 1);
                    }
                    else
                    {
                        _distance[facilityOne].Add(facilityTwo, int.MaxValue);
                    }
                }
            }
        }

        /// <summary>
        /// Megpróbálja visszaadni az épülethez legközelebb lévő szabad karbantartót, ha nincs akkor null-al tér vissza ezzel várakoztatva az épületet.
        /// </summary>
        /// <param name="building">Javításra szoruló épület</param>
        /// <param name="maintenanceWorkers">Karbantartók listája</param>
        /// <returns>A legközelebbi szabad karbantartó (ha van)</returns>
        public MaintenanceWorker? GetClosestMaintenanceWorker(Building building, List<MaintenanceWorker> maintenanceWorkers)
        {
            long closestWorkerDistance = int.MaxValue;
            MaintenanceWorker? closestMaintenanceWorker = maintenanceWorkers.FirstOrDefault();

            if (closestMaintenanceWorker == null) return null;
            foreach(MaintenanceWorker worker in maintenanceWorkers)
            {
                long distance = _distance[building][GetFacility(worker.Location)];
                if (worker.Status is WorkerStatus.Wandering && distance < closestWorkerDistance)
                {
                    closestMaintenanceWorker = worker;
                    closestWorkerDistance = distance;
                }
            }

            return closestMaintenanceWorker.Status is WorkerStatus.Wandering ? closestMaintenanceWorker : null;
        }

        /// <summary>
        /// Paraméterül kapott épületen kívűl visszaadja az összes játékot.
        /// Hogy mégegyszer ne választhassuk ugyanazt az épületet.
        /// </summary>
        /// <param name="exceptBuilding">A játékos aktuális épülete</param>
        /// <returns>Játékok gyűjteménye</returns>
        public ObservableCollection<Building> GetGames(Building exceptBuilding)
        {
            return FilterFacilities<Game>(exceptBuilding);
        }

        /// <summary>
        /// Paraméterül kapott épületen kívűl visszaadja az összes éttermet.
        /// Hogy mégegyszer ne választhassuk ugyanazt az épületet.
        /// </summary>
        /// <param name="exceptBuilding">A játékos aktuális épülete</param>
        /// <returns>Éttermek gyűjteménye</returns>
        public ObservableCollection<Building> GetFoodBuildings(Building exceptBuilding)
        {
            return FilterFacilities<FoodBuilding>(exceptBuilding);
        }

        /// <summary>
        /// Kódismétlés csökkentése érdekében a template paraméterként kapott épület típusokat szűri ki.
        /// </summary>
        /// <typeparam name="FilterClass">Épület típus</typeparam>
        /// <param name="exceptBuilding">A játékos aktuális épülete</param>
        /// <returns>Az épület típusnak megfeleltetett épületek</returns>
        private ObservableCollection<Building> FilterFacilities<FilterClass>(Building exceptBuilding)
        {
            if (Facilities == null)
                return null!;
            ObservableCollection<Building> filteredBuildings = new ObservableCollection<Building>();

            foreach (Facility facility in Facilities)
            {
                if (facility != exceptBuilding && facility is FilterClass)
                {
                    var tmp = facility as Building;
                    filteredBuildings.Add(tmp!);
                }
            }
            return filteredBuildings;
        }

        /// <summary>
        /// A megadott épületek közül kiválaszt egyet (félig véletlenszerűen).
        /// Az olcsóbb épületeket nagyobb hajlandósággal választja mint a drágábbakat.
        /// </summary>
        /// <param name="buildings">Az adott épületek</param>
        /// <returns>A kiválasztott épület</returns>
        public Building? ChooseBuilding(ObservableCollection<Building> buildings)
        {
            Building? selectedBuilding;
            int sumOfTicketPrices = 0;
            foreach(Building building in buildings) 
            {
                sumOfTicketPrices += building.TicketPrice / 10;
            }

            var sortedBuildings = buildings.OrderBy(b => b.TicketPrice).ToList();

            List<IntervalDatas> datas = new List<IntervalDatas>();

            int previousIntervalStart = 0;
            for(int i = 0; i < sortedBuildings.Count; ++i)
            {
                int intervalEnd = previousIntervalStart + sortedBuildings[i].TicketPrice / 10 - 1;
                datas.Add(new IntervalDatas(previousIntervalStart, intervalEnd, sortedBuildings[sortedBuildings.Count - 1 - i]));
                previousIntervalStart = intervalEnd + 1;
            }

            Random rnd = new Random();

            int choose = rnd.Next(0,sumOfTicketPrices);

            int j = 0;
            while(j < datas.Count && !(datas[j].LowerBound <= choose  && datas[j].UpperBound >= choose))
            {
                j++;
            }
            if (datas.Count == 0 && j == 0)
                return null;
            selectedBuilding = buildings[j];

            return selectedBuilding.Status.IsUsable() ? selectedBuilding : null;
        }

        /// <summary>
        /// Visszaadja a játékoshoz legközelebbi épületet.
        /// </summary>
        /// <param name="from">Játékos aktuális épülete</param>
        /// <returns>A legközelebbi épület</returns>
        public Building? GetClosestBuilding(Building from)
        {
            Building? closestBuilding = _distance[from].Keys.Where(f => f is Building && (f as Building) is not Gate && (f as Building) != from).FirstOrDefault() as Building;
            if (closestBuilding == null)
                return null;

            long closestBuildingDistance = _distance[from][closestBuilding];

            foreach(KeyValuePair<Facility,long> iterator in _distance[from])
            {
                if(iterator.Value != 0 && (iterator.Key is Game || iterator.Key is FoodBuilding) 
                   && iterator.Value < closestBuildingDistance)
                {
                    closestBuilding = iterator.Key as Building;
                    closestBuildingDistance = iterator.Value;
                }
            }
            if (closestBuilding == from || !closestBuilding!.Status.IsUsable())
                return null;

            return closestBuilding;
        }

        /// <summary>
        /// Visszaadja a listából a legolcsóbb épületet a belépője alapján.
        /// </summary>
        /// <param name="buildings">Épületek listája</param>
        /// <returns>Legolcsóbb épület</returns>
        public Building? GetCheapestDestination(ObservableCollection<Building> buildings)
        {
            Building? cheapestDestination = buildings.Count == 0 ? null : buildings.First();
            if (cheapestDestination == null)
                return null;
            foreach(Building building in buildings)
            {
                if((building is Game || building is FoodBuilding ) && building.TicketPrice < cheapestDestination.TicketPrice &&
                    (building.Status == FacilityStatus.Working || building.Status == FacilityStatus.Waiting))
                {
                    cheapestDestination = building;
                }
            }

            return cheapestDestination.Status.IsUsable() ? cheapestDestination : null;
        }

        /// <summary>
        /// Megadja két pont között az utat, ha létezik.
        /// Ha nem akkor hibát dob.
        /// </summary>
        /// <param name="from">Honnan</param>
        /// <param name="to">Hova</param>
        /// <returns>Útvonal</returns>
        /// <exception cref="ArgumentNullException">Hibás kérés, a két pont között nem létezik útvonal</exception>
        public List<Facility> GetPath(Facility from, Facility to)
        {
            if (to is null)
                throw new ArgumentNullException();
            List<Facility> path = new List<Facility>();
            Facility? tmp = to;
            path.Add(tmp);
            while (!tmp.Equals(from))
            {
                tmp = _pi[from][tmp];
                if(tmp is null)
                {
                    throw new ArgumentNullException();
                }
                path.Add(tmp);
            }
            path.Reverse();
            return path;
        }

        /// <summary>
        /// Visszaadja, hogy az adott építmény körül mennyi növény található és az azok által definiált pluszpontokat összeadja és
        /// visszaadja, hogy mennyivel növelje a játékos kedvét
        /// </summary>
        /// <param name="currentBuildingOfVisitor">Játékos aktuális épülete</param>
        /// <returns>Boldogság pontok</returns>
        public int GetHappiness(Facility currentBuildingOfVisitor)
        {
            int happinessBonus = 0;

            List<GridPoint> neighbours = new List<GridPoint>();
            //felső és alsó szomszédok
            for(int i = currentBuildingOfVisitor.Location.Column; i < currentBuildingOfVisitor.Location.Column + currentBuildingOfVisitor.Width; i++)
            {
                neighbours.Add(new GridPoint(currentBuildingOfVisitor.Location.Row - 1, i));
                neighbours.Add(new GridPoint(currentBuildingOfVisitor.Location.Row + currentBuildingOfVisitor.Height,i));
            }

            //jobb és bal oldali szomszédok
            for(int i = currentBuildingOfVisitor.Location.Row; i < currentBuildingOfVisitor.Location.Row + currentBuildingOfVisitor.Height; i++)
            {
                neighbours.Add(new GridPoint(i, currentBuildingOfVisitor.Location.Column + currentBuildingOfVisitor.Width));
                neighbours.Add(new GridPoint(i, currentBuildingOfVisitor.Location.Column - 1));
            }

            var plants = Facilities!.Where(f => f is Plant);

            foreach(var plant in plants)
            {
                if (neighbours.Contains(plant.Location))
                {
                    happinessBonus += (plant as Plant)!.HappinessFactor;
                }
            }

            return happinessBonus;
        }

        /// <summary>
        /// Megadja az adott koordinátán lévő épületet
        /// </summary>
        /// <param name="location">Koordináta</param>
        /// <returns>Az adott koodinátán lévő épület</returns>
        public Facility GetFacility(GridPoint location)
        {
            foreach(var facility in Facilities!)
            {
                if (facility.LocationOnMap.Equals(location) || facility.Location.Equals(location))
                {
                    return facility;
                }
            }
            //nem éri el ezt a kódrészletet a program
            return null!;
        }

        /// <summary>
        /// Megadja azokat az épületeket amikhez a megadott épületből vezet út.
        /// </summary>
        /// <param name="facility">Kérdezett épület</param>
        /// <returns>Úttal rendelkező épületek listája</returns>
        public List<Facility> GetBuildingsWithRoadToThis(Facility facility)
        {
            if (_distance.Count == 0) return new List<Facility>();
            List<Facility> buildingsWithRoad = new List<Facility>();
            foreach(KeyValuePair<Facility,long> pair in _distance[facility])
            {
                if(pair.Key is Building && pair.Value < int.MaxValue && pair.Key != facility)
                {
                    buildingsWithRoad.Add((Building)pair.Key);
                }
            }
            return buildingsWithRoad;
        }

        /// <summary>
        /// Karbantartó véletlenszerű célválasztásához a megadott pozíción kívűl választ egy másik utat véletlenszerűen.
        /// </summary>
        /// <param name="currentFacility">Karbantartó aktuális helye</param>
        /// <returns>A következő cél</returns>
        public Facility GetRandomRoad(Facility currentFacility)
        {
            Facility randomChoosenFacility;
            Random rnd = new Random();

            do
            {
                int index = rnd.Next(0, Facilities!.Count);
                randomChoosenFacility = Facilities![index];
            }while (randomChoosenFacility == currentFacility || randomChoosenFacility is not Road);

            return randomChoosenFacility;
        }

        /// <summary>
        /// Floyd-Warhsall algoritmus
        /// </summary>
        /// <param name="facilities">A gréf csúcspontjai</param>
        public void FloydWarshall(ObservableCollection<Facility> facilities)
        {
            if(_instance == null)
            {
                return;
            }
            MakeGraph(facilities);

            foreach (Facility i in facilities)
            {
                _pi.Add(i, new Dictionary<Facility, Facility?>());
                foreach(Facility j in facilities)
                {
                    if (!i.Equals(j) && _distance[i][j] < int.MaxValue)
                    {
                        _pi[i].Add(j, i);
                    }
                    else
                    {
                        _pi[i].Add(j, null);
                    }
                }
            }

            foreach(Facility k in facilities)
            {
                foreach(Facility i in facilities)
                {
                    foreach(Facility j in facilities)
                    {
                        if(_distance[i][j] > _distance[i][k] + _distance[k][j] && k is Road)
                        {
                            _distance[i][j] = _distance[i][k] + _distance[k][j];
                            _pi[i][j] = _pi[k][j];
                            if(i.Equals(j) && _distance[i][i] < 0)
                            {
                                // negatív kört találtunk, de az itt nem lehetséges mert az összes létező él 1 súlyú
                            }
                        }
                    }
                }
            }
            FloydWarshallDone?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Floyd-Warshall lefutott jelzése
        /// </summary>
        public event EventHandler? FloydWarshallDone;
    }
}
