using Google.Protobuf.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CollectionsExtensions {

    public static RepeatedField<T> CopyRepeatedField<T>(this RepeatedField<T> pattern)
    {
        if (pattern == null)
        {
            return null;
        }
        RepeatedField<T> result = new RepeatedField<T>();
        foreach(T item in pattern)
        {
            result.Add(item);
        }
        return result;
    }
}
