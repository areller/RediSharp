using System;
using System.Collections.Generic;

namespace RediSharp.Generator
{
    public static class DictionaryExtensions
    {
        public static TValue GetOrAdd<TValue, TKey>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> valueFactory)
        {
            if (!dictionary.TryGetValue(key, out var value))
            {
                value = valueFactory(key);
                dictionary.Add(key, value);
            }

            return value;
        }
    }
}
