using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Accessibility;

namespace Model.Util;


/// <summary>
/// Az ObservableCollection osztályhoz készült segédmetódusok.
/// </summary>
public static class ObservableCollectionExtensions
{
    /// <summary>
    /// Egy ObservableCollection példányt State példánnyá alakít az adott transzformáció mentén.
    /// Ha az ObservableCollection példány értéke változik, akkor változik a State példány is.
    /// </summary>
    /// <param name="oc">az ObservableCollection példány</param>
    /// <param name="transform">a transzformáció</param>
    /// <typeparam name="T">A State értékének típusa</typeparam>
    /// <typeparam name="T2">Az ObservableCollection elemeinek típusa</typeparam>
    /// <returns>A State példány</returns>
    public static State<T> AsState<T, T2>(this ObservableCollection<T2> oc, Func<ObservableCollection<T2>, T> transform)
    {
        var state = new State<T>(transform(oc));
        oc.CollectionChanged += (_, e) => state.Value = transform(oc);
        return state;
    }

    /// <summary>
    /// A LINQ-ban található Aggregate metódust valósítja meg, de ha oc értéke változik, akkor újra lefut a függvény
    /// </summary>
    /// <param name="oc">az ObservableCollection példány</param>
    /// <param name="def">az aggregálás kezdőértéke</param>
    /// <param name="acc">az akkumuláló függvény</param>
    /// <typeparam name="T">az ObservableCollection példányban található elemek</typeparam>
    /// <typeparam name="T2">az új State értékének típusa</typeparam>
    /// <returns></returns>
    public static State<T2> Aggregate<T, T2>(this ObservableCollection<State<T>> oc, T2 def, Func<T, T2, T2> acc)
    {
        var state = new State<T2>(def);
        FoldInline(oc, state, def, acc);
        oc.CollectionChanged += (_, _) => FoldInline(oc, state, def, acc);
        return state;
    }

    private static void FoldInline<T, T2>(this ObservableCollection<State<T>> oc, State<T2> state, T2 def, Func<T, T2, T2> acc)
    {
        var val = def;
        foreach (var item in oc)
        {
            val = acc(item.Value, val);
            item.ValueChanged += (_, _) => oc.FoldInline(state, def, acc);
        }

        state.Value = val;
    }
    
    /// <summary>
    /// A LINQ Select metódusának implementálása, de ha egy elemet hozzáadnak, vagy elvesznek az eredeti oc-ból,
    /// akkor az az új ObservableCollection példányban is megjelenik vagy eltűnik.
    /// </summary>
    /// <param name="oc">Az eredeti ObservableCollection példány</param>
    /// <param name="transform">az elemek transzformációja</param>
    /// <typeparam name="T">oc elemeinek típusa</typeparam>
    /// <typeparam name="T2">a létrejövő ObservableCollection típusa</typeparam>
    /// <returns>a létrejövő ObservableCollection típus</returns>
    public static ObservableCollection<T2> Map<T, T2>(this ObservableCollection<T> oc, Func<T, T2> transform)
    {
        var oc2 = new ObservableCollection<T2>(oc.Select(transform));
        oc.CollectionChanged += (_, e) =>
        {
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    oc2.Add(transform((T) item));
                }
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    oc2.Remove(transform((T) item));
                }
            }

        };
        return oc2;
    }

    /// <summary>
    /// Minden elemen egyszer lefutó mellékhatás hozzáadása oc-hez
    /// </summary>
    /// <param name="oc">egy ObservableCollection példány</param>
    /// <param name="action">a kívánt mellékhatás</param>
    /// <typeparam name="T">oc elemeinek típusa</typeparam>
    /// <returns>oc</returns>
    public static ObservableCollection<T> Do<T>(this ObservableCollection<T> oc, Action<T> action)
    {
        foreach (var item in oc)
        {
            action(item);
        }

        oc.CollectionChanged += (_, e) =>
        {
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    action((T) item);
                }
            }
        };
        return oc;
    }
    
    /// <summary>
    /// Összefűzi o1-et és o2-t egy ObservableCollection példányba. Ha hozzáadunk vagy elveszünk elemeket o1-ből vagy o2-ből,
    /// akkor az hozzáadódik/kitörlődik a létrejövő példányból is
    /// </summary>
    /// <param name="o1">egy ObservableCollection példány</param>
    /// <param name="o2">egy másik ObservableCollection példány</param>
    /// <typeparam name="T">az elemek típusa</typeparam>
    /// <returns>az új ObservableCollection példány</returns>
    public static ObservableCollection<T> Merge<T>(this ObservableCollection<T> o1, ObservableCollection<T> o2)
    {
        var o3 = new ObservableCollection<T>(o1);
        foreach (var item in o2)
        {
            o3.Add(item);
        }
        o1.CollectionChanged += (_, e) =>
        {
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    o3.Add((T) item);
                }
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    o3.Remove((T) item);
                }
            }
        };
        o2.CollectionChanged += (_, e) =>
        {
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    o3.Add((T) item);
                }
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    o3.Remove((T) item);
                }
            }
        };
        return o3;
    }

    /// <summary>
    /// Feliratkoztatja c1 változásaira c2-t.
    /// </summary>
    /// <param name="c1">egy ObservableCollection példány</param>
    /// <param name="c2">egy másik ObservableCollection példány</param>
    /// <typeparam name="T">az elemek típusa</typeparam>
    public static void Subscribe<T>(this ObservableCollection<T> c1, ObservableCollection<T> c2)
    {
        c2.Clear();
        foreach (var item in c1)
        {
            c2.Add(item);
        }

        c1.CollectionChanged += (_, e) =>
        {
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    c2.Add((T)item);
                }
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    c2.Remove((T) item);
                }
            }
        };
    }
}