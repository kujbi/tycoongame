namespace Model
{
    /// <summary>
    /// Az éttermek ősosztálya
    /// </summary>
    public abstract class FoodBuilding : Building
    {
        protected FoodBuilding(string name, GridPoint location, int width, int height, int activityDuration=10) : base(name, location, width, height,activityDuration)
        {
        }

        /// <summary>
        /// Az étterem ennyivel tölti vissza a látogató éhségét.
        /// </summary>
        public int HungerFactor { get; set; }

    }
}
