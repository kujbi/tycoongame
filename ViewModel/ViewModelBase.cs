using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ViewModel
{
    /// <summary>
    /// Az INotifyPropertyChanged interfész implementációja
    /// </summary>
    public class ViewModelBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Akkor kell meghívni, ha megváltozik egy tulajdonság
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;
        
        /// <summary>
        /// Meghívja a PropertyChanged eseményt
        /// </summary>
        /// <param name="memberCalling">a megváltozott tulajdonság neve</param>
        public void OnPropertyChanged([CallerMemberName] string? memberCalling = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberCalling));
        }
    }
}
