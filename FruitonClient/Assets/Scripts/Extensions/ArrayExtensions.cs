using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using KVector2 = fruiton.dataStructures.Point;

public static class ArrayExtensions {

    public static fruiton.dataStructures.Point GetIndices<T>(this T[,] array, T item)
    {
        for (int i = 0; i < array.GetLength(0); i++)
            for (int j = 0; j < array.GetLength(1); j++)
            {
                if (item.Equals(array[i, j]))
                {
                    return new KVector2(i, j);
                }
            }
        return null;
    }

    public static bool Contains<T>(this T[,] array, T item)
    {
        foreach (T member in array)
        {
            if (member != null && member.Equals(item))
            {
                return true;
            }
        }
        return false;
    }



}
