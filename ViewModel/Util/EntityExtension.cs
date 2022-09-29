using Model;

namespace ViewModel.Util
{
    /// <summary>
    /// Segédmetódusok az Entity típushoz
    /// </summary>
    public static class EntityExtension
    {
        /// <summary>
        /// Az entitás típusától függően visszaadja az entitás képének elérési útját
        /// </summary>
        /// <param name="entity">az entitás</param>
        /// <returns>az elérési út</returns>
        public static string GetImagePath(this Entity entity)
        {
            const string baseUrl = "../Resources/Images/";
            return baseUrl + entity switch
            {
                Dodgem => "dodgem.png",
                Tree => "tree.png",
                FerrisWheel => "ferriswheel.png",
                Road => "stone.png",
                Gate => "gate.png",
                Visitor => "dummy-person.png",
                MaintenanceWorker => "dummy_maintenanceWorker.png",
                AimShooting => "aimshooting.png",
                PopCornStand => "popcornstand.png",
                Restaurant => "res.png",
                Plant => "plant.png",
                RollerCoaster => "rollercoaster.png",   
                IceCreamStand => "icecreamstand.png",
                MiniTrain => "minitrain.png",
                HuntedHouse => "huntedhouse.png",
                _ => (entity.Height, entity.Width) switch
                {
                    (1, 1) => "dummy500_500.png",
                    (1, 2) => "dummy1000_500.png",
                    (2, 1) => "dummy500_1000.png",
                    _ => "dummy500_500.png"
                }
            };
        }
    }
}
