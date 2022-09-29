namespace Model.Util;

/// <summary>
/// Segédmetódusok a FacilityStatus típushoz
/// </summary>
public static class FacilityStatusExtensions
{
    /// <summary>
    /// true-t ad vissza, ha a státusz Working vagy Waiting
    /// </summary>
    /// <param name="status">a státusz</param>
    /// <returns>a státusz használható értéket mutat-e</returns>
    public static bool IsUsable(this FacilityStatus status)
    {
        return status is FacilityStatus.Working or FacilityStatus.Waiting;
    }
}