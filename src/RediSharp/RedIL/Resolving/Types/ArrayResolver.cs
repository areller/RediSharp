using System;

namespace RediSharp.RedIL.Resolving.Types
{
    class ArrayResolver : TypeResolver<Array>
    {
        class ArrayProxy
        {
            
        }

        public ArrayResolver()
        {
            Proxy<ArrayProxy>();
        }
    }
}