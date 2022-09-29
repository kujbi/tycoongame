using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// A perzisztencia interfésze
    /// </summary>
    public interface IPersistence
    {
        /// <summary>
        /// Ha már el volt mentve a játék, akkor a mentés elérési útja
        /// </summary>
        string? Path { get; }

        /// <summary>
        /// Betölt egy Parkot egy adott elérési útról
        /// </summary>
        /// <param name="path">az elérési út</param>
        /// <returns>a park</returns>
        Task<Park> LoadGameAsync(string path);

        
        /// <summary>
        /// Elmenti a Park példányt az adott elérési útra
        /// </summary>
        /// <param name="path">az elérési út</param>
        /// <param name="park">a park</param>
        void SaveGameAsync(string path, Park park);
        
        /// <summary>
        /// Törli az elérési útat
        /// </summary>
        void Reset();
    }
}