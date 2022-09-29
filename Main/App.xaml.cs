using Model;
using Persistence;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using View;
using View.Util;
using ViewModel.Main;
using ViewModel.MainMenu;
using TimeMode = Model.TimeMode;

namespace Main
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IPersistence _persistence;
        private readonly GameModel _model;

        private readonly MainViewModel _mainViewModel;
        private readonly MainWindow _mainWindow;

        private readonly MainMenuViewModel _mainMenuViewModel;
        private readonly MainMenuWindow _mainMenuWindow;

        private readonly DispatcherTimer _timer;

        public App()
        {
            _persistence = new XmlPersistence();
            _model = new GameModel(_persistence);

            _mainViewModel = new MainViewModel(_model);
            _mainWindow = new MainWindow();

            _mainMenuViewModel = new MainMenuViewModel();
            _mainMenuWindow = new MainMenuWindow();

            _timer = new DispatcherTimer();

            Startup += App_Startup;
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            
            
            _mainViewModel.MenubarViewModel.MainMenuButtonClicked += MainViewModel_MainMenuButtonClicked;
            _mainViewModel.MenubarViewModel.SaveCommandExecuted += MainViewModel_SaveCommandExecuted;
            _mainViewModel.MenubarViewModel.OpenCommandExecuted += MainViewModel_OpenCommandExecuted;
            _mainViewModel.MenubarViewModel.StartOrStopCommandExecuted += MenubarViewModelOnStartOrStopCommandExecuted;
            
            _mainWindow.DataContext = _mainViewModel;
            _mainWindow.Closing += MainWindow_Closing;
            _mainWindow.Closed += Window_Closed;
            

            _mainMenuViewModel.ExitCalled += MainMenuViewModel_ExitCalled;
            _mainMenuViewModel.NewGameCalled += MainMenuViewModel_NewGameCalled;
            _mainMenuViewModel.OpenGameCommandExecuted += MainMenuViewModel_OpenGameCommandExecuted;

            _mainMenuWindow.DataContext = _mainMenuViewModel;
            _mainMenuWindow.Closed += Window_Closed;


            _mainMenuWindow.Show();
            
            _model.ParkState.SelectMany(p => p.TimeModeState).ValueChanged +=
                Park_TimeModeChanged;
            _model.Park.GameOver += ParkGameOver;
            _model.ParkState.ValueChanged += ParkState_ValueChanged;
            


            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
        }

        private void ParkState_ValueChanged(object? sender, Park e)
        {
            _model.Park.GameOver += ParkGameOver;
        }

        private void ParkGameOver(object? sender, bool e)
        {
            if (!_mainWindow.IsVisible) return;

            var result = MessageBox.Show(
               caption: "Vége a játéknak!",
               icon: MessageBoxImage.Exclamation,
               button: MessageBoxButton.OK,
               defaultResult: MessageBoxResult.OK,
               owner: _mainWindow,
               messageBoxText: "Csóró ,de csóró vagyok én.",
               options: MessageBoxOptions.None
               );

            _mainWindow.Hide();
            _mainMenuWindow.Show();
        }

        private void Park_TimeModeChanged(object? sender, TimeMode e)
        {
            _timer.Interval = e switch
            {
                TimeMode.Slow => TimeSpan.FromSeconds(2),
                TimeMode.Normal => TimeSpan.FromSeconds(1),
                TimeMode.Fast => TimeSpan.FromSeconds(0.5),
                _ => throw new ArgumentOutOfRangeException(nameof(e), e, null)
            };
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            _model.TimeAdvanced();
        }

        private void MenubarViewModelOnStartOrStopCommandExecuted(object? sender, EventArgs e)
        {
            _model.OpenOrClosePark();
            //if (_timer.IsEnabled)
            //    _timer.Stop();
            //else
            //    _timer.Start();
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            if (!_mainWindow.IsVisible) return;

            var result = MessageBox.Show(
                caption: "Kilépés megerősítése",
                icon: MessageBoxImage.Question,
                button: MessageBoxButton.YesNoCancel,
                defaultResult: MessageBoxResult.Yes,
                owner: _mainWindow,
                messageBoxText: "Szeretnéd menteni a játékot?",
                options: MessageBoxOptions.None
                );

            switch (result)
            {
                case MessageBoxResult.Cancel:
                    e.Cancel = true;
                    break;
                case MessageBoxResult.Yes:
                    SaveGame();
                    break;
                case MessageBoxResult.No:
                    break;
            }
        }

        private void MainMenuViewModel_OpenGameCommandExecuted(object? sender, EventArgs e)
        {
            var result = OpenGame();
            if(result)
            {
                _mainMenuWindow.Hide();
                _mainWindow.Show();
                if (!_timer.IsEnabled)
                    _timer.Start();
            }

        }

        private void MainViewModel_OpenCommandExecuted(object? sender, EventArgs e)
        {
            OpenGame();
        }

        private void MainViewModel_SaveCommandExecuted(object? sender, EventArgs e)
        {
            SaveGame();
        }

        private void MainViewModel_MainMenuButtonClicked(object? sender, EventArgs e)
        {
            if (!_mainWindow.IsVisible) return;

            var needToSave = MessageBox.Show(
                caption: "Kilépés megerősítése",
                icon: MessageBoxImage.Question,
                button: MessageBoxButton.YesNoCancel,
                defaultResult: MessageBoxResult.Yes,
                owner: _mainWindow,
                messageBoxText: "Szeretnéd menteni a játékot?",
                options: MessageBoxOptions.None
                );

            if (needToSave == MessageBoxResult.Cancel) return;

            if (needToSave == MessageBoxResult.Yes)
            {
                var couldSave = SaveGame();
                if (!couldSave) return;
            }
            
            _mainWindow.Hide();
            _mainMenuWindow.Show();
        }

        private void MainMenuViewModel_NewGameCalled(object? sender, string e)
        {
            _persistence.Reset();
            _model.NewGame(parkName: e);
            _mainMenuWindow.Hide();
            _mainWindow.Show();
            if(!_timer.IsEnabled)
                _timer.Start();
        }

        private void MainMenuViewModel_ExitCalled(object? sender, EventArgs e)
        {
            Shutdown();
        }

        private void Window_Closed(object? sender, EventArgs e)
        {
            Shutdown();
        }

        private bool SaveGame()
        {
            var path = _persistence.Path ?? FilePickers.ShowSavePicker();
            if (path != null)
            {
                _model.SaveGameAsync(path);
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool OpenGame()
        {
            var path = FilePickers.ShowOpenPicker();
            if (path == null) return false;
            _model.OpenGameAsync(path);
            return true;

        }
    }
}
