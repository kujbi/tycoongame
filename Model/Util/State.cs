using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Model.Util;

/// <summary>
/// Egy konténer, ami értesíti a rá feliratkozókat, ha megváltozik az értéke.
/// </summary>
/// <typeparam name="T">az érték típusa</typeparam>
public class State<T> :  INotifyPropertyChanged
{
    private T _value;
    
    /// <summary>
    /// Inicializál egy új állapot példányt value alapértékkel
    /// </summary>
    /// <param name="value">az érték</param>
    public State(T value)
    {
        _value = value;
    }
    
    /// <summary>
    /// Az állapot értéke
    /// </summary>
    public T Value
    {
        get => _value;
        set
        {
            if (Equals(_value, value)) return;
            _value = value;
            ValueChanged?.Invoke(this, value);
            OnPropertyChanged();
        }
    }
    
    /// <summary>
    /// Létrehoz egy új állapot példányt aminek az értéke megváltozik, ha az eredeti példány értéke is megváltozik.
    /// </summary>
    /// <param name="transform">az eredeti és az új állapot értékei közti transzformáció</param>
    /// <typeparam name="T2">Az új állapot értékének típusa</typeparam>
    /// <returns>az új állapot</returns>
    public State<T2> Select<T2>(Func<T, T2> transform)
    {
        var state = new State<T2>(transform(Value));
        ValueChanged += (sender, e) => state.Value = transform(e);
        return state;
    }

    /// <summary>
    /// Létrehoz egy új állapot példányt aminek az értéke megváltozik, ha az eredeti példány értéke is megváltozik és
    /// fordítva.
    /// </summary>
    /// <remarks>transform és inverse függvényeknek egymás inverzeinek kell lenniük</remarks>
    /// <param name="transform">transzformáció az eredeti állapot értékéből az új állapot értékébe</param>
    /// <param name="inverse">transzformáció az új állapot értékéből az eredeti állapot értékébe</param>
    /// <typeparam name="T2">az új állapot értékének típusa</typeparam>
    /// <returns>az új állapot</returns>
    /// <exception cref="Exception">ha a transform és inverse nem inverzei egymásnak</exception>
    public State<T2> SelectTwoWay<T2>(Func<T, T2> transform, Func<T2, T> inverse)
    {
        if (!Equals(inverse(transform(Value)), Value))
            throw new Exception("The inverse parameter must be the inverse if the transform function");

        var state = Select(transform);
        state.ValueChanged += (_, v) => Value = inverse(v);
        return state;
    }

    /// <summary>
    /// Feliratkoztatja az other-t ennek a State-nek az változására
    /// </summary>
    /// <param name="other">egy másik State példány</param>
    public void Subscribe(State<T> other)
    {
        Subscribe(other, x => x);
    }
    
    private void Subscribe<T2>(State<T2> other, Func<T, T2> transform)
    {
        other.Value = transform(Value);
        ValueChanged += (_, _) => other.Value = transform(Value);
    }

    /// <summary>
    /// Összerakja a State-t es az other-t egy új State példánnyá. Ha ez vagy other értéke
    /// megváltozik, akkor az új példány értéke újraszámolódik
    /// </summary>
    /// <param name="other">a másik State példány</param>
    /// <param name="transform">a transzformáció</param>
    /// <typeparam name="T2">other értékének típusa</typeparam>
    /// <typeparam name="T3">az új State értékének típusa</typeparam>
    /// <returns>az új State példány</returns>
    public State<T3> Combine<T2, T3>(State<T2> other, Func<T, T2, T3>  transform)
    {
        var state = new State<T3>(transform(Value, other.Value));
        ValueChanged += (_, v) => state.Value = transform(v, other.Value);
        other.ValueChanged += (_, v) => state.Value = transform(Value, v);
        return state;
    }
    
    /// <summary>
    /// létrehoz egy új State példányt. Ha az eredeti State értéke megváltozik, vagy ha a transform-tól eredményül kapott
    /// State értéke megváltozik, akkor frissül az új példány is.
    /// </summary>
    /// <param name="transform">a transzformáció</param>
    /// <typeparam name="T2">az új State értékének típusa</typeparam>
    /// <returns>az új State példány</returns>
    public State<T2> SelectMany<T2>(Func<T, State<T2>> transform)
    {
        var state = transform(Value);
        ValueChanged += (_, v) =>
        {
            transform(v).Subscribe(state);
        };
        return state;
    }

    /// <summary>
    /// létrehoz egy új ObservableCollection példányt. Ha az eredeti State értéke megváltozik, vagy ha a transform-tól eredményül kapott
    /// ObservableCollection értéke megváltozik, akkor frissül az új példány is.
    /// </summary>
    /// <param name="transform">a transzformáció</param>
    /// <typeparam name="T2">az új ObservableCollection értékeinek típusa</typeparam>
    /// <returns>az új ObservableCollection</returns>
    public ObservableCollection<T2> SelectMany<T2>(Func<T, ObservableCollection<T2>> transform)
    {
        var oc = new ObservableCollection<T2>(transform(Value));
        transform(Value).Subscribe(oc);
        ValueChanged += (_, v) =>
        {
            oc.Clear();
            foreach (var item in transform(v))
            {
                oc.Add(item);
            }
            transform(v).Subscribe(oc);
        };
        return oc;

    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// akkor hívódik meg, megváltozik a State értéke
    /// </summary>
    public event EventHandler<T>? ValueChanged;
    
    /// <summary>
    /// akkor hívódik meg, megváltozik a State értéke
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;
}