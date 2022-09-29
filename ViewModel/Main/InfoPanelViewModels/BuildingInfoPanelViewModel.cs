using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using Model;
using Model.Util;
using ViewModel.Util;

namespace ViewModel.Main.InfoPanelViewModels;

/// <summary>
/// Épületek információs panele
/// </summary>
public class BuildingInfoPanelViewModel : ViewModelBase, IInfoPanelViewModel
{
    /// <summary>
    /// Inicializál egy új BuildingInfoPanelViewModel példányt
    /// </summary>
    /// <param name="cellViewModel">az adatokat tartalmazó CellViewModel</param>
    /// <exception cref="ArgumentOutOfRangeException">Ha a CellViewModel nem Building típust tartalmaz</exception>
    public BuildingInfoPanelViewModel(CellViewModel cellViewModel)
    {
        if (cellViewModel.Entity is not Model.Building)
            throw new ArgumentOutOfRangeException(nameof(cellViewModel), "CellViewModel.EntityEntity is not Building"); 
        
        CellViewModel = cellViewModel;
        TicketPriceLabelState = Building.TicketPriceState.SelectTwoWay(i => i.ToString(), int.Parse);
        EditControlsVisibilityState = new State<Visibility>(Building is Gate ? Visibility.Collapsed : Visibility.Visible);
        TicketPrice = new MutableStringValue(TicketPriceLabelState);
        UtilizationProgressBarState = Building.UtilizationState.Select(u => (int)((double)u/Building.Capacity * 100));
        UtilizationNumberState = Building.UtilizationState.Select(u => $"{u}/{Building.Capacity}");
        QueueLengthState = Building.Queue.AsState(q => q.Count);
        HealthNumberState = Building.HealthState.Select(i => $"{i}/{Building.MaxHealth}");
        BuildAndRepairTimeProgressBarState =
            Building.BuildAndRepairTimeState.Select(i => (int) ((double) i / Building.BuildTime * 100));
        BuildAndRepairProgressBarVisibility =
            Building.StatusState.Select(s => s.IsUsable() ? Visibility.Hidden : Visibility.Visible);
        HealthProgressBarState = Building.HealthState.Select(i => (int)((double)i / Building.MaxHealth * 100));
        MinUtilizationPercentState = Building.MinimalUtilizationPercentState;
        MinUtilizationLabelState = MinUtilizationPercentState.Select(i => $"{i}%");
        StatusLabelState = Building.StatusState.Select(s => s switch
        {
            FacilityStatus.Broken => "Elromlott",
            FacilityStatus.Building => "Építés alatt",
            FacilityStatus.Repairing => "Javítás alatt",
            FacilityStatus.Waiting => "Várakozik",
            FacilityStatus.Working => "Használatban van",
            FacilityStatus.WaitingForMaintenanceWorker => "Szerelőre vár",
            _ => throw new ArgumentOutOfRangeException(nameof(s), s, null)
        });
    }

    private CellViewModel CellViewModel { get; }
    private Building Building => (Building) CellViewModel.Entity;
    
    /// <summary>
    /// Az épület képének elérési útja
    /// </summary>
    public string ImagePath => $"../{Building.GetImagePath()}";
    private State<string> TicketPriceLabelState { get; }
    
    /// <summary>
    /// Az belépőjegy ára
    /// </summary>
    public MutableStringValue TicketPrice { get; }

    /// <summary>
    /// Az szerkesztéshez szükséges eszközök láthatóságának állapota
    /// </summary>
    public State<Visibility> EditControlsVisibilityState { get; }

    /// <summary>
    /// Az épület épülésének vagy javításának állapota
    /// </summary>
    public State<int> BuildAndRepairTimeProgressBarState { get; }
    
    /// <summary>
    /// Az épület épülésállapot ProgressBar-jának láthatósága
    /// </summary>
    public State<Visibility> BuildAndRepairProgressBarVisibility { get; }
    
    /// <summary>
    /// A kihasználtság állapota (ProgressBar)
    /// </summary>
    public State<int> UtilizationProgressBarState { get; }
    
    /// <summary>
    /// A kihasználtság állapota (szöveges)
    /// </summary>
    public State<string> UtilizationNumberState { get; }
    
    /// <summary>
    /// Az épület státuszának állapota
    /// </summary>
    public State<string> StatusLabelState { get; }
    
    /// <summary>
    /// A sor hosszának állapota
    /// </summary>
    public State<int> QueueLengthState { get; }
    
    /// <summary>
    /// Az életerő ProgressBar állapota
    /// </summary>
    public State<int> HealthProgressBarState { get; }
    
    /// <summary>
    /// Az életerő szövegének állapota
    /// </summary>
    public State<string> HealthNumberState { get; }

    /// <summary>
    /// A minimális kihasználtság százalék állapota
    /// </summary>
    public State<int> MinUtilizationPercentState { get; }

    
    /// <summary>
    /// A minimális kihasználtság címkéjének állapota
    /// </summary>
    public State<string> MinUtilizationLabelState { get; }
}
