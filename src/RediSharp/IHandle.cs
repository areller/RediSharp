using System.Threading.Tasks;
using StackExchange.Redis;

namespace RediSharp
{
    public interface IHandle<TRes>
    {
        /// <summary>
        /// The artifact the the handle uses to execute the function (e.g. Lua code)
        /// </summary>
        object Artifact { get; }

        /// <summary>
        /// Is the handle initialized
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Initializes the handle (e.g. SCRIPT LOAD)
        /// </summary>
        /// <returns>A task for awaiting the initialization of the handle</returns>
        Task Init();

        /// <summary>
        /// Executes the function that is wrapped by the handle. Works only when the handle is initialized.
        /// </summary>
        /// <param name="args">Argument</param>
        /// <param name="keys">Redis keys</param>
        /// <exception cref="HandleException">Thrown when the handle is not initialized</exception>
        /// <returns></returns>
        Task<TRes> Execute(RedisValue[] args, RedisKey[] keys);
    }
}