using System.Reflection;
using StackExchange.Redis;

namespace RediSharp
{
    public class SEClient : Client<IDatabase>
    {
        /// <summary>
        /// Creates a new instance of Client with an IDatabase cursor
        /// </summary>
        /// <param name="db">A connection instance to a Redis database</param>
        public SEClient(IDatabase db)
            : base(db, db)
        {
        }

        
        internal SEClient(IDatabase db, Assembly assembly)
            : base(db, assembly, db)
        {
        }
    }
}