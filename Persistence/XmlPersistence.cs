using Model;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Persistence
{
    /// <summary>
    /// az IPersistence interfész implementációja XML serializer-rel
    /// </summary>
    public class XmlPersistence : IPersistence
    {
        private readonly XmlSerializer _serializer;
        
        /// <summary>
        /// létrehoz egy új XmlPersistence példányt
        /// </summary>
        public XmlPersistence()
        {
            _serializer = new XmlSerializer(typeof(Park));
        }

        
        /// <summary>
        /// Ha már el volt mentve a játék, akkor a mentés elérési útja
        /// </summary>
        public string? Path { get; private set; }

        /// <summary>
        /// Betölt egy Parkot egy adott elérési útról
        /// </summary>
        /// <param name="path">az elérési út</param>
        /// <returns>a park</returns>
        public async Task<Park> LoadGameAsync(string path)
        {
            await using var fr = new FileStream(path, FileMode.Open);
            var park = (Park)(_serializer.Deserialize(fr) ?? throw new NullReferenceException());
            Path = path;
            return park;
        }

        /// <summary>
        /// Törli az elérési útat
        /// </summary>
        public void Reset()
        {
            Path = null;
        }
        
        /// <summary>
        /// Elmenti a Park példányt az adott elérési útra
        /// </summary>
        /// <param name="path">az elérési út</param>
        /// <param name="park">a park</param>
        public async void SaveGameAsync(string path, Park park)
        {
            await using (var fw = new FileStream(path, FileMode.Create))
            {
                _serializer.Serialize(fw, park);
            }

            Path = path;
        }
    }
}
