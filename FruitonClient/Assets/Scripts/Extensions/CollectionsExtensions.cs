using System;
using Google.Protobuf.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CollectionsExtensions {

    public static RepeatedField<T> Copy<T>(this RepeatedField<T> pattern)
    {
        RepeatedField<T> result = new RepeatedField<T>();
        foreach(T item in pattern)
        {
            result.Add(item);
        }
        return result;
    }
}
