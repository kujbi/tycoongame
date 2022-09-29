using Model;
using Model.Util;
using ViewModel.Util;

namespace ViewModel.Main.InfoPanelViewModels;

/// <summary>
/// A látogatók info panele
/// </summary>
public class VisitorInfoPanelViewModel : ViewModelBase, IInfoPanelViewModel
{
    /// <summary>
    /// Inicializál egy új VisitorInfoPanelViewModel példányt
    /// </summary>
    /// <param name="cellViewModel">az adatokat tartalmazó viewModel</param>
    public VisitorInfoPanelViewModel(CellViewModel cellViewModel)
    {
        CellViewModel = cellViewModel;
        HappinessState = Visitor.HappinessState;
        HungerState = Visitor.HungerState;
        MoneyState = Visitor.MoneyState;
    }


    private CellViewModel CellViewModel { get; }
    private Visitor Visitor => (Visitor) CellViewModel.Entity;
    
    /// <summary>
    /// A látogató képének elérési útja
    /// </summary>
    public string ImagePath => $"../{Visitor.GetImagePath()}";
    
    /// <summary>
    /// A boldogság állapota
    /// </summary>
    public State<int> HappinessState { get; }
    
    /// <summary>
    /// Az éhség állapota
    /// </summary>
    public State<int> HungerState { get; }
    
    /// <summary>
    /// A látogató pénzének állapota
    /// </summary>
    public State<int> MoneyState { get; }
}