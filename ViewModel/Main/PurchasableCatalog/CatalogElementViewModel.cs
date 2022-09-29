using System;
using Model;
using ViewModel.Util;

namespace ViewModel.Main.PurchasableCatalog
{
    /// <summary>
    /// A katalógus elem kiválaszthatóságának fajtái
    /// </summary>
    public enum SelectionType { None, Selected, SelectedPermanent }

    /// <summary>
    /// A katalógusban található elemek nézetmodellje
    /// </summary>
    public class CatalogElementViewModel : ViewModelBase
    {
        private SelectionType _selectionType;

        /// <summary>
        /// A kiválasztás típusa
        /// </summary>
        public SelectionType SelectionType { 
            get => _selectionType;
            set
            {
                _selectionType = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// A termék képének elérési útja
        /// </summary>
        public string ImagePath => Purchasable.GetImagePath();
        
        /// <summary>
        /// A termék ára
        /// </summary>
        public string Price => Purchasable.Price + " Ft";
        
        
        /// <summary>
        /// A termék neve
        /// </summary>
        public string Name => Purchasable.Name;
        
        /// <summary>
        /// A termék
        /// </summary>
        public Purchasable Purchasable { get; }

        /// <summary>
        /// A kattintás parancs
        /// </summary>
        public DelegateCommand ClickCommand { get; }
        
        /// <summary>
        /// A dupla kattintás parancs
        /// </summary>
        public DelegateCommand DoubleClickCommand { get; }

        /// <summary>
        /// Inicializál egy új CatalogElementViewModel példányt
        /// </summary>
        /// <param name="purchasable">A megvásárolható elem</param>
        /// <param name="selectionType">A kiválasztás típusa</param>
        public CatalogElementViewModel(Purchasable purchasable, SelectionType selectionType = SelectionType.None)
        {
            Purchasable = purchasable;
            ClickCommand = new DelegateCommand(_ => CatalogElement_Clicked());
            DoubleClickCommand = new DelegateCommand(_ => CatalogElement_DoubleClicked());
            SelectionType = selectionType;
        }

        /// <summary>
        /// A kijelölés visszavonása
        /// </summary>
        public void Deselect()
        {
            SelectionType = SelectionType.None;
        }

        
        private void CatalogElement_DoubleClicked()
        {
            SelectionType = SelectionType.SelectedPermanent;
            SelectedPermanently?.Invoke(this, this);
        }

        private void CatalogElement_Clicked()
        {
            SelectionType = SelectionType.Selected;
            Selected?.Invoke(this, this);
        }

        
        /// <summary>
        /// Akkor hívódik meg, ha kiválasztják az elemet
        /// </summary>
        public event EventHandler<CatalogElementViewModel>? Selected;
        
        /// <summary>
        /// Akkor hívódik meg, ha hosszan kiválasztják az elemet
        /// </summary>
        public event EventHandler<CatalogElementViewModel>? SelectedPermanently;
    }
}
