using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ArrayExtensions {

    public static Tuple<int, int> GetIndices<T>(this T[,] array, T item)
    {
        for (int i = 0; i < array.GetLength(0); i++)
            for (int j = 0; j < array.GetLength(1); j++)
            {
                if (item.Equals(array[i, j]))
                {
                    return Tuple.New(i, j);
                }
            }
        return null;
    }

}
