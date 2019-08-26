using System;
using System.Collections.Generic;

namespace RediSharp.RedIL
{
    class KeyValuePairEqualityComparer<K, V> : IEqualityComparer<KeyValuePair<K, V>>
        where K : IEquatable<K>
        where V : IEquatable<V>
    {
        public bool Equals(KeyValuePair<K, V> x, KeyValuePair<K, V> y)
        {
            return ((IEquatable<K>) x.Key).Equals(x.Key) && ((IEquatable<V>) x.Value).Equals(x.Value);
        }

        public int GetHashCode(KeyValuePair<K, V> obj)
        {
            return obj.GetHashCode();
        }
    }
}