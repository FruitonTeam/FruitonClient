﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LazyDictionary<TKey, TValue> : Dictionary<TKey, TValue> where TValue : new()
{
    public new TValue this[TKey key]
    {
        get
        {
            if (!ContainsKey(key))
            {
                this[key] = new TValue();
            }
            return base[key];
        }
        set
        {
            base[key] = value;
        }
    }
}
