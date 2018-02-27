using System.Collections.Generic;

namespace DataStructures
{
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
}
