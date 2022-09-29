using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Util
{
    /// <summary>
    /// Intervallum adatok osztálya
    /// </summary>
    public readonly struct IntervalDatas
    {
        /// <summary>
        /// Alsó határ
        /// </summary>
        public Int32 LowerBound { get; }

        /// <summary>
        /// Felső határ
        /// </summary>
        public Int32 UpperBound { get; }

        /// <summary>
        /// Az intervallumhoz tartozó épület
        /// </summary>
        public Building Building { get; }

        /// <summary>
        /// Paraméteres konstruktor
        /// </summary>
        /// <param name="lowerBound">Alső határ</param>
        /// <param name="upperBound">Felső határ</param>
        /// <param name="building">Az intervallumhoz tartozó épület</param>
        public IntervalDatas(int lowerBound, int upperBound, Building building)
        {
            LowerBound = lowerBound;
            UpperBound = upperBound;
            Building = building;
        }
    }
}

