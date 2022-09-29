using System;
using System.Collections.ObjectModel;
using System.Linq;
using Model;
using ViewModel.Util;

namespace ViewModel.Main.PurchasableCatalog
{
    /// <summary>
    /// A katalógus nézetmodellje
    /// </summary>
    public class PurchasableCatalogViewModel : ViewModelBase
    {
        private CatalogElementViewModel? _selectedElement;

        /// <summary>
        /// Inicializál egy új PurchasableCatalogViewModel példányt
        /// </summary>
        /// <param name="model"></param>
        public PurchasableCatalogViewModel(GameModel model)
        {
            model.ParkState.ValueChanged += Model_ParkChanged;

            PermanentSelectionQuitCommand = new DelegateCommand(
                _ => SelectedElement!=null && SelectedElement.SelectionType == SelectionType.SelectedPermanent,
                _ => {  DeselectAll(); SelectedElement = null; });
        }

        /// <summary>
        /// A katalógus elemei
        /// </summary>
        public ObservableCollection<CatalogElementViewModel> Elements { get; } = new();

        /// <summary>
        /// A kiválasztott elem
        /// </summary>
        public CatalogElementViewModel? SelectedElement 
        { 
            get => _selectedElement;
            private set { 
                if(value != _selectedElement)
                {
                    _selectedElement = value;
                    SelectedElementChanged?.Invoke(this, value);
                }   
            } 
        }

        /// <summary>
        /// Hosszútávú kiválasztás megszüntetése parancs
        /// </summary>
        public DelegateCommand PermanentSelectionQuitCommand { get; }

        /// <summary>
        /// Összes elem kiválasztásának megszüntetése
        /// </summary>
        public void Deselect()
        {
            DeselectAll();
        }

        private void Model_ParkChanged(object? sender, Park e)
        {
            Elements.Clear();
            foreach (var purchasable in e.PurchasableCatalog)
            {
                var ce = new CatalogElementViewModel(purchasable);
                ce.Selected += Element_Selected;
                ce.SelectedPermanently += Element_Selected;
                Elements.Add(ce);
            }
        }

        private void Element_Selected(object? sender, CatalogElementViewModel e)
        {

            DeselectAllExcept(e);
            SelectedElement = MakeInstance(e);
            SelectedElement.SelectionType = e.SelectionType;
        }

        private void DeselectAllExcept(CatalogElementViewModel cevm)
        {
            foreach(var elem in Elements)
            {
                if(elem != cevm) elem.Deselect();
            }
        }

        public void ElementHasBeenPutDown()
        {
            if(SelectedElement == null) throw new ArgumentNullException(nameof(SelectedElement));
            if(SelectedElement.SelectionType == SelectionType.SelectedPermanent)
            {
                SelectedElement = MakeInstance(SelectedElement);
            }
            else
            {
                DeselectAll();
                SelectedElement = null;
            }
        }

        private void DeselectAll()
        { 
            foreach (var elem in Elements)
            {
                elem.Deselect();
            }

            SelectedElement = null;
        }

        private CatalogElementViewModel MakeInstance(CatalogElementViewModel cevm)
        {
            return new CatalogElementViewModel(
                cevm.Purchasable
                .GetType()
                .GetConstructors()
                .FirstOrDefault(c => c.GetParameters().Length == 0)?
                .Invoke(null) as Purchasable ?? throw new Exception(), cevm.SelectionType);

        }
        
        
        /// <summary>
        /// Akkor hívódik meg, ha megváltozik a kiválasztott elem
        /// </summary>
        public event EventHandler<CatalogElementViewModel?>? SelectedElementChanged;
    }

}
