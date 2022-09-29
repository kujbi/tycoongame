using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Util;

namespace Model
{
    /// <summary>
    /// Az idő folyásának gyorsasága
    /// </summary>
    public enum TimeMode { Slow, Normal, Fast}

    
    /// <summary>
    /// A játék modell osztálya
    /// </summary>
    public class GameModel
    {
        private readonly IPersistence _persistence;

        /// <summary>
        /// Inicializál egy GameModel példányt
        /// </summary>
        /// <param name="persistence">Az a perzisztencia példány, amit a modell használ a mentésnél</param>
        public GameModel(IPersistence persistence)
        {
            _persistence = persistence;
        }

        /// <summary>
        /// A park állapota
        /// </summary>
        public State<Park> ParkState { get; } = new(new Park());

        /// <summary>
        /// A park
        /// </summary>
        public Park Park
        {
            get => ParkState.Value;
            set => ParkState.Value = value;
        }

        /// <summary>
        /// Ez a metódus minden ticknél meghívódik. Ez felelős az automatikus folyamatok működéséért.
        /// </summary>
        public void TimeAdvanced()
        {
            if (Map.Instance.Facilities is null)
                return;
            Park.TimeAdvanced();
        }

        /// <summary>
        /// Új parkot hoz létre
        /// </summary>
        /// <param name="parkName">az új park neve</param>
        public void NewGame(string parkName)
        {
            Park = new Park(name: parkName, budget: 1_000_000, Model.ParkStatus.Closed);
        }

        /// <summary>
        /// A park elmentése.
        /// </summary>
        /// <param name="path">a fájl elérési útja, ahova menteni akarunk</param>
        public void SaveGameAsync(string path)
        {
            _persistence.SaveGameAsync(path, Park);
        }

        /// <summary>
        /// Megnyit egy mentést
        /// </summary>
        /// <param name="path">a fájl, ahonnan be akarjuk olvasni a mentést</param>
        public async void OpenGameAsync(string path)
        {
            var park = await _persistence.LoadGameAsync(path);
            Park = park;
            Park.ParkStatus = ParkStatus.Closed;
        }

        /// <summary>
        /// Megnyitja vagy bezárja a parkot a státuszától függően
        /// </summary>
        public void OpenOrClosePark()
        {
            Park.OpenOrClose();
        }
        
    }
}
