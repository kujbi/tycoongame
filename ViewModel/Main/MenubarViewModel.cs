using Model;
using System;
using System.Linq;
using Model.Util;
using ViewModel.Util;

namespace ViewModel.Main
{
    /// <summary>
    /// A menüsor nézetmodellje
    /// </summary>
    public class MenubarViewModel : ViewModelBase
    {
        /// <summary>
        /// Inicializál egy új MenubarViewModel példányt
        /// </summary>
        /// <param name="model">a játékmodell</param>
        public MenubarViewModel(GameModel model)
        {
            Model = model;

            IsSlowSelectedState = Model.ParkState.SelectMany(p => p.TimeModeState)
                .Select(s => s == TimeMode.Slow);
            IsNormalSelectedState = Model.ParkState.SelectMany(p => p.TimeModeState)
                .Select(s => s == TimeMode.Normal);
            IsFastSelectedState = Model.ParkState.SelectMany(p => p.TimeModeState)
                .Select(s => s == TimeMode.Fast);
            SaveCommandCanExecuteState = Model.ParkState.SelectMany(p => p.ParkStatusState)
                .Select(s => s == ParkStatus.Closed);
            

            StartButtonLabelState = Model.ParkState
                .SelectMany(p => p.ParkStatusState)
                .Select(s => s == ParkStatus.Closed ? "Park megnyitása" : "Park bezárása");

            MainMenuCommand = new DelegateCommand(_ => MainMenuButtonClicked?.Invoke(this, EventArgs.Empty));
            SaveGameCommand = new DelegateCommand(
                SaveCommandCanExecuteState, 
                _ => SaveCommandExecuted?.Invoke(this, EventArgs.Empty));
            OpenGameCommand = new DelegateCommand(_ => OpenCommandExecuted?.Invoke(this, EventArgs.Empty));
            
            StartCommand = new DelegateCommand(
                 _ => Gate.Instance.GetBuildingsWithRoadToThis(Map.Instance).Any(), 
                _ => StartCommand_Executed());

            SlowButtonCommand = new DelegateCommand(
                Model.ParkState.SelectMany(p => p.TimeModeState).Select(s => s != TimeMode.Slow), 
                _ => Model.Park.TimeMode = TimeMode.Slow);

            NormalButtonCommand = new DelegateCommand(
                Model.ParkState.SelectMany(p => p.TimeModeState).Select(s => s != TimeMode.Normal), 
                _ => Model.Park.TimeMode = TimeMode.Normal);

            FastButtonCommand = new DelegateCommand(
                Model.ParkState.SelectMany(p => p.TimeModeState).Select(s => s != TimeMode.Fast), 
                _ => Model.Park.TimeMode = TimeMode.Fast);
            
            Map.Instance.FloydWarshallDone += (_, _) => StartCommand.RaiseCanExecuteChanged();
        }

        private GameModel Model { get; }
        
        /// <summary>
        /// A főmenü parancs
        /// </summary>
        public DelegateCommand MainMenuCommand { get; }
        
        /// <summary>
        /// A játék megnyitása parancs
        /// </summary>
        public DelegateCommand OpenGameCommand { get; }
        
        /// <summary>
        /// A mentés parancs
        /// </summary>
        public DelegateCommand SaveGameCommand { get; }
        
        /// <summary>
        /// A lassú gomb parancs
        /// </summary>
        public DelegateCommand SlowButtonCommand { get; }
        
        /// <summary>
        /// A normál gomb parancs
        /// </summary>
        public DelegateCommand NormalButtonCommand { get; }
        
        /// <summary>
        /// A gyors gomb parancs
        /// </summary>
        public DelegateCommand FastButtonCommand { get; }
        
        /// <summary>
        /// Az indítás parancs
        /// </summary>
        public DelegateCommand StartCommand { get; }

        /// <summary>
        /// A lassú gomb kijelölésének állapota
        /// </summary>
        public State<bool> IsSlowSelectedState { get; }
        
        /// <summary>
        /// A normál gomb kijelölésének állapota
        /// </summary>
        public State<bool> IsNormalSelectedState { get; }
        
        /// <summary>
        /// A gyors gomb kijelölésének állapota
        /// </summary>
        public State<bool> IsFastSelectedState { get; }
        
        /// <summary>
        /// A mentés gomb használhatóságának állapota
        /// </summary>
        private State<bool> SaveCommandCanExecuteState { get; }
        
        /// <summary>
        /// Az indítás gomb címkéjének állapota
        /// </summary>
        public State<string> StartButtonLabelState { get; }

        private void StartCommand_Executed()
        {
            StartOrStopCommandExecuted?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Akkor fut le, ha a főmenü gombot megnyomják
        /// </summary>
        public event EventHandler? MainMenuButtonClicked;
        
        /// <summary>
        /// Akkor fut le, ha a mentés gombot megnyomják
        /// </summary>
        public event EventHandler? SaveCommandExecuted;
        
        /// <summary>
        /// Akkor fut le, ha a megnyitás gombot megnyomják
        /// </summary>
        public event EventHandler? OpenCommandExecuted;
        
        /// <summary>
        /// Akkor fut le, ha az indítás gombot megnyomják
        /// </summary>
        public event EventHandler? StartOrStopCommandExecuted;
    }
}