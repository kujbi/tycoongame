using System;
using Model;
using Model.Util;
using ViewModel.Util;

namespace ViewModel.Main.InfoPanelViewModels;

/// <summary>
/// A MaintenanceWorker info panele
/// </summary>
public class MaintenanceWorkerInfoPanel : IInfoPanelViewModel
{
    /// <summary>
    /// Inicializál egy új MaintenanceWorkerInfoPanel példányt
    /// </summary>
    /// <param name="cellViewModel">az adatokat tartalmazó CellViewModel</param>
    /// <exception cref="ArgumentOutOfRangeException">ha a CellViewModel nem MaintenanceWorker típust tartalmaz</exception>
    public MaintenanceWorkerInfoPanel(CellViewModel cellViewModel)
    {
        CellViewModel = cellViewModel;
        StatusLabelState = MaintenanceWorker.StatusState.Select(s => s switch
        {
            WorkerStatus.Wandering => "Vakarja a fejét",
            WorkerStatus.Working => "Dolgozik",
            WorkerStatus.WanderingToWork => "Javítani megy",
            _ => throw new ArgumentOutOfRangeException(nameof(s), s, null)
        });
    }

    private CellViewModel CellViewModel { get; }
    
    /// <summary>
    /// Az épület képének elérési útja
    /// </summary>
    public string ImagePath => $"../{MaintenanceWorker.GetImagePath()}";
    private MaintenanceWorker MaintenanceWorker => (MaintenanceWorker) CellViewModel.Entity;
    
    /// <summary>
    /// A státusz állapota
    /// </summary>
    public State<string> StatusLabelState { get; }
}