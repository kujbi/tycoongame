using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Controls;
using Model;
using Model.Util;
using ViewModel.Main.InfoPanelViewModels;
using ViewModel.Main.PurchasableCatalog;
using ViewModel.Util;

namespace ViewModel.Main
{
    /// <summary>
    /// A főablak nézetmodellje
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        
        private CellViewModel? _previewCell;
        private CellViewModel? _selectedEntityViewModel;
        
        /// <summary>
        /// Inicializál egy új MainViewModel példányt
        /// </summary>
        /// <param name="model">a modell</param>
        public MainViewModel(GameModel model)
        {
            Model = model;
            Model.ParkState.ValueChanged += Park_Changed;

            PurchasableCatalogViewModel = new PurchasableCatalogViewModel(model);
            PurchasableCatalogViewModel.SelectedElementChanged += FacilityCatalogViewModelOnSelectedElementChanged;
            MenubarViewModel = new MenubarViewModel(model);
            InfoPanelViewModelState.Value = new DefaultInfoPanelViewModel();

            EscPressedCommand = new DelegateCommand(EscPressedExecuted);
            DeleteCommand = new DelegateCommand(_ => SelectedEntityViewModel != null, DeleteCommandExecuted);
            
            Cells = new ObservableCollection<CellViewModel>();

            TimeState = Model.ParkState.SelectMany(p => p.TimeState);
            BudgetState = Model.ParkState.SelectMany(p => p.BudgetState);
            NameState = Model.ParkState.SelectMany(p => p.NameState);
            FieldSizeState = Model.ParkState.SelectMany(p => p.SizeState);
            VisitorNumberState = Model.ParkState
                .SelectMany(p => p.Visitors)
                .AsState(c => c.Count);

            
            
            Cells.Do(c => c.DeleteExecuted += CellViewModelDeleteExecuted);
            
            Park_Changed(this, Model.Park);
        }

        /// <summary>
        /// A modell
        /// </summary>
        public GameModel Model { get; }

        /// <summary>
        /// A park budget állapota
        /// </summary>
        public State<int> BudgetState { get; }
        
        /// <summary>
        /// Az idő állapota
        /// </summary>
        public State<TimeSpan> TimeState { get; }
        
        /// <summary>
        /// A név állapota
        /// </summary>
        public State<string> NameState { get; }
        
        /// <summary>
        /// A pálya méretének állapota
        /// </summary>
        public State<int> FieldSizeState { get; }

        
        private CellViewModel? PreviewCell
        {
            get => _previewCell;
            set 
            {
                if (_previewCell != value)
                {
                    if (_previewCell != null) Cells.Remove(_previewCell);
                    if (value != null)
                    {
                        value.IsPreview = true;
                        Cells.Add(value);
                    }
                    _previewCell = value;
                }
            }
        }

        private CellViewModel? SelectedEntityViewModel
        {
            get => _selectedEntityViewModel;
            set
            {
                if (_selectedEntityViewModel != null) _selectedEntityViewModel.IsSelected = false;

                if (value == null) InfoPanelViewModelState.Value = new DefaultInfoPanelViewModel();
                else
                {
                    value.IsSelected = true;
                    InfoPanelViewModelState.Value = value.Entity switch
                    {
                        Building => new BuildingInfoPanelViewModel(value),
                        Visitor => new VisitorInfoPanelViewModel(value),
                        MaintenanceWorker => new MaintenanceWorkerInfoPanel(value),
                        _ => new DefaultInfoPanelViewModel()
                    };
                }
                
                _selectedEntityViewModel = value;
            }
        }
        
        /// <summary>
        /// A játéktéren található entitások
        /// </summary>
        public ObservableCollection<CellViewModel> Cells { get; }

        /// <summary>
        /// A látogatók számának állapota
        /// </summary>
        public State<int> VisitorNumberState { get; }
        
        
        /// <summary>
        /// A katalógus nézetmodellje
        /// </summary>
        public PurchasableCatalogViewModel PurchasableCatalogViewModel { get; }

        /// <summary>
        /// A menü nézetmodellje
        /// </summary>
        public MenubarViewModel MenubarViewModel { get; }

        /// <summary>
        /// Az info panel állapota
        /// </summary>
        public State<IInfoPanelViewModel> InfoPanelViewModelState { get; } = new(new DefaultInfoPanelViewModel());
        
        /// <summary>
        /// Az Esc gomb megnyomása parancs
        /// </summary>
        public DelegateCommand EscPressedCommand { get; }
        
        /// <summary>
        /// A törlés parancs
        /// </summary>
        public DelegateCommand DeleteCommand { get; }

        private void FacilityCatalogViewModelOnSelectedElementChanged(object? sender, CatalogElementViewModel? e)
        {
            if(e == null) RemovePreviewCellFromField();
        }

        private void DeleteCommandExecuted(object? obj)
        {
            SelectedEntityViewModel!.DeleteCommand.Execute(null);
        }

        private void EscPressedExecuted(object? obj)
        {
            PurchasableCatalogViewModel.PermanentSelectionQuitCommand.Execute(obj);
            PurchasableCatalogViewModel.Deselect();
            SelectedEntityViewModel = null;
        }

        private void Park_Changed(object? sender, Park e)
        {
            e.Facilities.CollectionChanged += EntityCollection_CollectionChanged;
            e.Visitors.CollectionChanged += EntityCollection_CollectionChanged;
            e.MaintenanceWorkers.CollectionChanged += EntityCollection_CollectionChanged;
            e.Visitors.CollectionChanged += Visitors_CollectionChanged;
            


            Cells.Clear();
            foreach (var facility in e.Facilities)
            {
                Cells.Add(new CellViewModel(facility, this));
            }

            foreach (var worker in e.MaintenanceWorkers)
            {
                Cells.Add(new CellViewModel(worker, this));
            }

        }

        private void Visitors_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            var visitors = sender as ObservableCollection<Visitor> ?? throw new InvalidCastException();
            //VisitorNumberLabel = visitors.Count.ToString();
        }

        private void EntityCollection_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach(var item in e.NewItems)
                {
                    var entity = (Entity)item;
                    var vm = new CellViewModel(entity, this);
                    vm.DeleteExecuted += CellViewModelDeleteExecuted;
                    Cells.Add(vm);
                }
            }

            if (e.OldItems != null)
            {
                if (SelectedEntityViewModel != null && e.OldItems.Contains(SelectedEntityViewModel.Entity))
                    SelectedEntityViewModel = null;
                foreach (var item in e.OldItems)
                {
                    var entity = (Entity)item;
                    var cvm = Cells.First(c => ReferenceEquals(c.Entity, entity));
                    Cells.Remove(cvm);
                }
            }
        }

        private void CellViewModelDeleteExecuted(object? sender, CellViewModel e)
        {
            Model.Park.RemovePurchasable((Purchasable) e.Entity);
        }

        /// <summary>
        /// Akkor fut le, ha a pálya valamilyen interakcióba lép az egérrel
        /// </summary>
        /// <param name="type">Az interakció típusa</param>
        /// <param name="point">Az interakció helye</param>
        public void Grid_CursorAction(MouseActionType type, GridPoint point)
        {
            if (PurchasableCatalogViewModel.SelectedElement != null)
            {
                
                var purchasable = PurchasableCatalogViewModel.SelectedElement.Purchasable;
                var placementResult = Model.Park.CanPurchasableBePlaced(purchasable, point);


                switch (type, placementResult)
                {
                    case (MouseActionType.Enter, PlacementResult.Valid or PlacementResult.Conflict):
                        purchasable.Location = point;
                        AddPreviewCellToField(purchasable);
                        break;

                    case (MouseActionType.Leave, _):
                        RemovePreviewCellFromField();
                        break;

                    case (MouseActionType.Click, PlacementResult.Valid):
                        purchasable.Location = point;
                        RemovePreviewCellFromField();
                        Model.Park.TryPlacingPurchasable(purchasable);
                        PurchasableCatalogViewModel.ElementHasBeenPutDown();
                        break;

                    case (MouseActionType.Click, _):
                        RemovePreviewCellFromField();
                        break;

                    case (MouseActionType.Move, PlacementResult.Valid or PlacementResult.Conflict):
                        if (PreviewCell == null)
                        {
                            AddPreviewCellToField(purchasable);
                        }
                        purchasable.Location = point;

                        break;
                }

                if (PreviewCell != null) PreviewCell.IsCollided = placementResult switch
                {
                    PlacementResult.Conflict => true,
                    _ => false
                };

            }
            else
            {
                if (type != MouseActionType.Click) return;

                var entityCellViewModel = Cells
                    .Where(c => c.ZIndexState.Value >= 0)
                    .OrderByDescending(c => c.ZIndexState.Value)
                    .FirstOrDefault(c => c.Entity.GetGridPoints().Contains(point));

                SelectedEntityViewModel = entityCellViewModel;
            }

        }

        private void AddPreviewCellToField(Purchasable purchasable)
        {
            var cvm = new CellViewModel(purchasable, this);
            PreviewCell = cvm;
        }

        private void RemovePreviewCellFromField()
        {
            PreviewCell = null;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
        }
    }
}
