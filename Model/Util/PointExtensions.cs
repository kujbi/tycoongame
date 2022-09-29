using System.Drawing;

namespace Model.Util
{
    /// <summary>
    /// Segédmetódusok a Point típushoz
    /// </summary>
    public static class PointExtensions
    {
        /// <summary>
        /// Átalakít egy Point típust GridPoint-tá
        /// </summary>
        /// <param name="point">a Point példány</param>
        /// <returns>a GridPoint példány</returns>
        public static GridPoint ToGridPoint(this Point point) => new GridPoint(point);
    }
}
