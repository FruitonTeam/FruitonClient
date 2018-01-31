using System;
using System.Collections.Generic;

public static class IListExtensions
{
    private static readonly Random rnd = new Random();

    public static T GetRandomElement<T>(this IList<T> list)
    {
        int rndIdx = rnd.Next(list.Count);
        return list[rndIdx];
    }

    public static T GetRandomElement<T>(this IList<T> list, Random randomGenerator)
    {
        int rndIdx = randomGenerator.Next(list.Count);
        return list[rndIdx];
    }
}

