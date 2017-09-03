using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class KernelCastExtensions {

    public static List<T> ToList<T>(this Array<T> array)
    {
        var list = new List<T>(array.length);
        for (int i = 0; i < array.length; i++)
        {
            list.Add(array[i]);
        }
        return list;
    }

    public static List<TOut> CastToList<TOut, TIn>(this Array<TIn> array)
        where TIn : class
        where TOut : TIn
    {
        var list = new List<TOut>(array.length);
        for (int i = 0; i < array.length; i++)
        {
            list.Add((TOut)array[i]);
        }
        return list;
    }

    public static List<TOut> CastToList<TOut>(this Array<object> array)
    {
        return array.CastToList<TOut, object>();
    }
}
