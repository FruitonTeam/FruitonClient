﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataStructures
{
    [Serializable]
    public class Tuple<T1, T2>
    {
        [SerializeField]
        public T1 First { get; private set; }
        [SerializeField]
        public T2 Second { get; private set; }
        internal Tuple(T1 first, T2 second)
        {
            First = first;
            Second = second;
        }
        public override string ToString()
        {
            return "First = " + First + " Second = " + Second;
        }
    }

    public static class Tuple
    {
        public static Tuple<T1, T2> New<T1, T2>(T1 first, T2 second)
        {
            var tuple = new Tuple<T1, T2>(first, second);
            return tuple;
        }
    }
}