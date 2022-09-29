using System;
using Model.Util;

namespace ViewModel.Util;

/// <summary>
/// Egy osztály amit meg lehet jeleníteni a nézettel és bele lehet írni egy string értéket
/// </summary>
public class MutableStringValue
{
    /// <summary>
    /// Inicializál egy MutableStringValue példányt
    /// </summary>
    /// <param name="stringState">egy string-et tároló State</param>
    public MutableStringValue(State<string> stringState)
    {
        TextState = stringState;
        SaveCommand = new DelegateCommand(s => TextState.Value = (string) s!);
    }
    
    /// <summary>
    /// Mentés parancs 
    /// </summary>
    public DelegateCommand SaveCommand { get; }
    
    /// <summary>
    /// A szöveg állapota
    /// </summary>
    public State<string> TextState { get; }
}