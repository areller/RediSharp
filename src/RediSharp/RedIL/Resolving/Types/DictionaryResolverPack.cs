using System;
using System.Collections.Generic;

namespace RediSharp.RedIL.Resolving.Types
{
    class DictionaryResolverPack
    {
        class DictionaryProxy<K, V>
        {
            
        }
        
        public static Dictionary<Type, Type> GetMapToProxy()
        {
            return new Dictionary<Type, Type>()
            {
                { typeof(Dictionary<,>), typeof(DictionaryProxy<,>) }
            };
        }
    }
}