using System;
using ViewModel.Util;

namespace ViewModel.MainMenu
{
    /// <summary>
    /// A főmenü ablak nézetmodellje
    /// </summary>
    public class MainMenuViewModel : ViewModelBase
    {
        /// <summary>
        /// Az új játék parancs
        /// </summary>
        public DelegateCommand NewGameCommand { get; }
        
        /// <summary>
        /// A megnyitás parancs
        /// </summary>
        public DelegateCommand OpenGameCommand { get; }
        
        /// <summary>
        /// A kilépés parancs
        /// </summary>
        public DelegateCommand ExitCommand { get; }

        /// <summary>
        /// Inicializál egy új MainMenuViewModel példányt
        /// </summary>
        public MainMenuViewModel()
        {
            NewGameCommand = new DelegateCommand(name => { NewGameCalled?.Invoke(this, (string)name!); });
            OpenGameCommand = new DelegateCommand(_ => OpenGameCommandExecuted?.Invoke(this, null!));
            ExitCommand = new DelegateCommand(_ => { ExitCalled?.Invoke(this, null!); });
        }

        /// <summary>
        /// Akkor hívódik meg, ha egy meglévő játékot akarunk megnyitni
        /// </summary>
        public event EventHandler? OpenGameCommandExecuted;
        
        /// <summary>
        /// Akkor hívódik meg, ha ki akarunk lépni
        /// </summary>
        public event EventHandler? ExitCalled;
        
        /// <summary>
        /// Akkor hívódik meg, ha új játékot akarunk kezdeni
        /// </summary>
        public event EventHandler<string>? NewGameCalled;
    }
}
