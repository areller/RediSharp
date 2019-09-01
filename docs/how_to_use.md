# How to Use
To use the library, you should create an instance of `Client<TCursor>`. 

`TCursor` is a cursor type. The cursor is equivalent to the `redis` variable in Lua, only it is strongly typed.  
Currently, the only cursor available is `IDatabase` (`StackExchange.Redis.IDatabase`).   

The `Client<TCursor>` constructor accepts an `IDatabase` instance from an existing Redis connection.

```C#
class Program
{
    static async Task Main(string[] args)
    {
        var connection = await ConnectionMultiplexer.ConnectAsync("localhost");
        var client = new Client<IDatabase>(connection.GetDatabase(0));
    }
}
```

Now that the client is set up, you can start loading Redis functions.  

Redis functions are stored as and accessed by an `IHandle`. Once an `IHandle` is created, it can be reused for however long the application runs.  

To create an handle, you use `client.GetLuaHandle` and provide it with a C# function, either in the form of a lambda function, or in the form of a pointer to a method.  

After you create the handle, you have to initialize it by calling `handle.Init` which returns a `Task` that you can await.  

Initializing the handle uploads the generated Lua to the Redis server via the `SCRIPT LOAD` command.  

```C#
class Program
{
    static bool RedisFunction(IDatabase cursor, RedisValue[] args, RedisKey[] keys)
    {
        return cursor.StringGet(keys[0]);
    }

    static async Task Main(string[] args)
    {
        ...
        var handle = client.GetLuaHandle(RedisFunction);
        await handle.Init();
    }
}
```

Once you have an initialized handle, you can start executing the function that it wraps by calling `handle.Execute`.  
You can also get the generated Lua from `handle.Artifact`.  

```C#
Console.WriteLine(handle.Artifact); // Prints the generated Lua
var res = handle.Execute(new RedisValue[]{}, new RedisKey[]{"myKey"});
```  

The return type of `Execute` matches the return type that was defined in the function given to `CreateLuaHandle` (`bool` in this case).  