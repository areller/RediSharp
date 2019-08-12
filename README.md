# RedSharper

RedSharper allows you to write C# code that will execute directly on the Redis server.  

It does that by [transpiling](https://en.wikipedia.org/wiki/Source-to-source_compiler) the C# code to Lua.

```C#
var res = await client.Execute((cursor, argv, keys) =>
            {
                var count = cursor.Get(keys[0]).AsInt();
                var toAdd = (int) argv[0];

                for (var i = 0; i < count; i++)
                {
                    var key = keys[0] + "_" + i;
                    var currentValue = cursor.Get(key).AsLong() ?? 0;
                    cursor.Set(key, currentValue + toAdd);
                }

                return RedResult.Ok;
            }, new RedisValue[] {5}, new RedisKey[] {"countKey"});
```

```LUA
local num = tonumber(redis.call('get', KEYS[1]));
local num2 = tonumber(ARGV[1]);
local i = 0;
while i<num do
    local key = tostring(KEYS[1].."_")..i;
    local num3 = (tonumber(redis.call('get', key)) or 0);
    redis.call('set', key, num3+num2)
    i = i+1
end
return { ok = 'OK' };
```

## TODO List

* Add more Redis commands
* Add ability to create and manipulate lists/dictionaries
* Support more of C#'s syntax
  * ForEach
  * Switch/Case
  * Custom methods (?)
  * Custom types (structs) (?)
* Some refactoring
* Document
* Write proper unit tests


## Source Guide

### Demo
[RedSharper.Demo](./tests/RedSharper.Demo) is a demo project that utilizes the library.  

### C# Code Decompilation
RedSharper uses [ILSpy](https://github.com/icsharpcode/ILSpy) to decompile compiled lambda function to a C# syntax tree.  
See the [CSharp](./src/RedSharper/CSharp) folder. 

### RedIL
[RedIL](./src/RedSharper/RedIL) is an intermidiate language that is created from C# code, and later compiled to Lua, and potentially other targets in the future (See "Future Plans").  

### Lua Compilation
Lua is written by traversing the RedIL using an [IRedILVisitor](./src/RedSharper/RedIL/IRedILVisitor.cs).  
See the [Lua](./src/RedSharper/Lua) folder. 

## Dependencies
* [ILSpy](https://github.com/icsharpcode/ILSpy) - MIT License
* [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis) - MIT License

## Future Plans

* Transpiling C# Code directly to C code that will run as a Redis module.

## Disclaimer
This library is still at it's early stage of development and not meant to be production ready yet. Use at your own risk.
