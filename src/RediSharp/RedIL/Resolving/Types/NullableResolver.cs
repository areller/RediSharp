using System;

namespace RediSharp.RedIL.Resolving.Types
{
    class NullableResolver<T> : TypeResolver<Nullable<T>>
        where T : struct
    {
        public NullableResolver()
        {
            
        }
    }
}