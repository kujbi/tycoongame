using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace View.Controls;

/// <summary>
/// Egy TextBox, ami csak számokat fogad
/// </summary>
public class NumberTextBox : TextBox
{
    /// <summary>
    /// Inicializál egy új NumberTextBox példányt
    /// </summary>
    public NumberTextBox()
    {
        PreviewTextInput += OnPreviewTextInput;
    }

    private static void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = Regex.IsMatch(e.Text, "^[^0-9]+$");
    }
}