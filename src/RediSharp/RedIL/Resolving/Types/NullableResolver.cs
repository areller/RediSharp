using System;

namespace RediSharp.RedIL.Resolving.Types
{
    // We use `int?` as a template nullable type
    class NullableResolver : TypeResolver<int?>
    {
        public NullableResolver()
        {
            
        }
    }
}