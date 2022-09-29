using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// Entitás lehelyezésénél fellépő kivétel
    /// </summary>
    public class PlacingException : Exception
    {
        /// <summary>
        /// A lehelyezés eredménye
        /// </summary>
        private PlacementResult PlacementResult { get; set; }

        /// <summary>
        /// Inicializál egy új PlacingException-t
        /// </summary>
        /// <param name="result">A lehelyezés eredménye</param>
        /// <param name="message">A hibaüzenet</param>
        public PlacingException(PlacementResult result, string? message = "") : base(message)
        {
            PlacementResult = result;
        }
    }
}
