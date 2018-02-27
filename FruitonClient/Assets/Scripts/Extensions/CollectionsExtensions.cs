using System.Collections.Generic;
using System.Text;
using Google.Protobuf.Collections;

namespace Extensions
{
    public static class CollectionsExtensions
    {
        public static RepeatedField<T> Copy<T>(this RepeatedField<T> pattern)
        {
            var result = new RepeatedField<T>();
            foreach(T item in pattern)
            {
                result.Add(item);
            }
            return result;
        }

        public static string ToDebugString<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            var stringBuilder = new StringBuilder();
            foreach (KeyValuePair<TKey, TValue> kvPair in dictionary)
            {
                stringBuilder.Append("(").Append(kvPair.Key).Append(", ").Append(kvPair.Value).Append(") ");
            }
            return stringBuilder.ToString();
        }
    }
}
