using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace View.Util
{
    /// <summary>
    /// Fájlkiválasztó dialógusablakok
    /// </summary>
    public static class FilePickers
    {
        /// <summary>
        /// Mentés ablak megnyitása
        /// </summary>
        /// <returns>az elérési út</returns>
        public static string? ShowSavePicker()
        {
            var picker = new SaveFileDialog()
            {
                Filter = "Vidámpark fájl (.rtx)|*.rtx|Minden fájl|*.*",
                AddExtension = true,
                OverwritePrompt = true,
                DefaultExt = "rtx",
                RestoreDirectory = true,
                Title = "Játék mentése"
            };

            var result = picker.ShowDialog();
            if (result == true)
            {
                return picker.FileName;
            }
            return null;
        }

        /// <summary>
        /// Megnyitás ablak megnyitása
        /// </summary>
        /// <returns>az elérési út</returns>
        public static string? ShowOpenPicker()
        {
            var picker = new OpenFileDialog()
            {
                Filter = "Vidámpark fájl (.rtx)|*.rtx|Minden fájl|*.*",
                AddExtension = true,
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = "rtx",
                RestoreDirectory = true,
                Title = "Játék megnyitása"
            };

            var result = picker.ShowDialog();
            if (result == true)
            {
                return picker.FileName;
            }
            return null;
        }
    }
}
