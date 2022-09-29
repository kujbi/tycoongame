using System;
using System.Windows;
using Model;
using Model.Util;
using ViewModel.Util;

namespace ViewModel.Main
{
    /// <summary>
    /// Egy parkban lévő entitás nézetmodellje
    /// </summary>
    public class CellViewModel : ViewModelBase
    {
        private bool _isCollided;
        private bool _isPreview;
        private bool _isSelected;

        /// <summary>
        /// Inicializál egy új CellViewModel példányt
        /// </summary>
        /// <param name="entity">az entitás</param>
        /// <param name="mainViewModel">a mainViewModel, amiben ez a példány is van</param>
        public CellViewModel(Entity entity, MainViewModel mainViewModel)
        {
            Entity = entity;
            MainViewModel = mainViewModel;
            IsCollided = false;
            IsSelected = false;

            DeleteCommand = new DelegateCommand(DeleteCommand_CanExecute,_ => DeleteExecuted?.Invoke(this, this));
            RowState = Entity.LocationState.Select(p => p.Row);
            ColState = Entity.LocationState.Select(p => p.Column);
            ZIndexState = Entity is Visitor visitor
                ? visitor.StatusState.Select(s => s == VisitorStatus.Wandering ? 10 : -10) : new State<int>(entity switch
                {
                    Facility => 1,
                    MaintenanceWorker => 10,
                    _ => throw new ArgumentOutOfRangeException(nameof(entity), entity, null)
                });


            if (Entity is Visitor visitor2) VisibilityState = visitor2.StatusState.Select(s => s == VisitorStatus.Wandering ? Visibility.Visible : Visibility.Collapsed);
            else VisibilityState = new State<Visibility>(Visibility.Visible);
        }

        /// <summary>
        /// Az entitás
        /// </summary>
        public Entity Entity { get; }
        
        private MainViewModel MainViewModel { get; }

        /// <summary>
        /// Az entitás képének az elérési útja
        /// </summary>
        public string ImagePath => Entity.GetImagePath();
        
        /// <summary>
        /// Megadja, hogy csak előnézetként van-e lent a CellViewModel
        /// </summary>
        public bool IsPreview { get => _isPreview; set { _isPreview = value; OnPropertyChanged(); } }
        
        /// <summary>
        /// Megadja, hogy ütközik-e másik CellViewModellel
        /// </summary>
        public bool IsCollided
        {
            get => _isCollided;
            set { _isCollided = value; OnPropertyChanged(); }
        }
        
        /// <summary>
        /// Megadja, hogy ki van e választva a példány
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }
        
        /// <summary>
        /// A sor állapota
        /// </summary>
        public State<int> RowState { get; }
        
        /// <summary>
        /// Az oszlop állapota
        /// </summary>
        public State<int> ColState { get; }
        
        /// <summary>
        /// A láthatóság állapota
        /// </summary>
        public State<Visibility> VisibilityState { get; }
        
        /// <summary>
        /// A sorösszevonás
        /// </summary>
        public int RowSpan => Entity.Height;
        
        /// <summary>
        /// Az oszlopösszevonás
        /// </summary>
        public int ColSpan => Entity.Width;

        /// <summary>
        /// A törlés parancs
        /// </summary>
        public DelegateCommand DeleteCommand { get; }

        /// <summary>
        /// A ZIndex állapota
        /// </summary>
        public State<int> ZIndexState { get; }
        private bool DeleteCommand_CanExecute(object? obj)
        {
            return MainViewModel.Model.Park.ParkStatus == ParkStatus.Closed && Entity is not Gate;
        }

        
        /// <summary>
        /// Akkor hívódik meg, ki akarjuk törölni az elemet
        /// </summary>
        public event EventHandler<CellViewModel>? DeleteExecuted;
    }
}
