# RediSharp

This project is currently under development, stay tuned :)

![](https://travis-ci.com/areller/RediSharp.svg?branch=master)    

[![Nuget](https://img.shields.io/nuget/v/RediSharp)](https://www.nuget.org/packages/RediSharp)

RediSharp allows you to write C# code that will execute directly on the Redis server.  

It does that by [transpiling](https://en.wikipedia.org/wiki/Source-to-source_compiler) the C# code to Lua.

```C#
var dict = new Dictionary<string, List<int>>()
{
    {"abc", new List<int>() {1, 2, 3}},
    {"cde", new List<int>() {3, 4, 5}}
};

foreach (var elem in dict)
{
    cursor.SetAdd("names", elem.Key);
    foreach (var num in elem.Value)
    {
        cursor.SetAdd($"{elem.Key}_nums", num);
    }
}

var union = cursor.SetCombine(SetOperation.Union, new RedisKey[] {"abc_nums", "cde_nums"});
var ts = TimeSpan.FromSeconds((int?) cursor.StringGet("exp") ?? 5);
cursor.StringSet("json", Json.Encode(union), ts);

return union;
```

```LUA
local dictionary = {["abc"]={1, 2, 3}, ["cde"]={3, 4, 5}};
for _1,_2 in pairs(dictionary) do
 local item = {_1,_2};
 redis.pcall("SADD", "names", item[1])
 for _,item2 in ipairs(item[2]) do
  redis.pcall("SADD", item[1].."_nums", item2)
 end
end
local array = redis.pcall("SUNION", "abc_nums", "cde_nums");
local value = 1000*((redis.pcall("GET", "exp") or 5));
if value==nil then
 redis.pcall("SET", "json", cjson.encode(array))
elseif value~=nil then
 redis.pcall("SET", "json", cjson.encode(array), "PX", value)
end
return array;

```

## More Documents
* [How to Use](./docs/how_to_use.md)
* [Debugging](./docs/debugging.md)

## Contributing
There are currently no exact guidelines/rules for contributing, but it's always welcome :)  
Feel free to clone the repository, play with it, open issues, submit pull requests or email me at areller.gm@gmail.com

## Why?

When we execute Lua scripts from C#, we lose a lot of the advanges that the C# compiler and the IDE offer, such as auto completion, compile-time error checking, debugging, and many more.  

RediSharp aims to mitigate these issues.

## Supports

* Primitive Types, Arrays, Dictionaries
* Conditions, Switch Statements, For/While/ForEach loops
* Anonymous Types
* TimeSpan
* String Operations (Join, Split, etc...)
* Math (Not All)
* Json
* Redis Commands: Strings, Lists, Hashes, Sets, Sorted Sets (Not All)

## TODO List

* Add more Redis commands (Complete Sorted Sets support, HyperHyperLog, Streams, etc...)
* Support more of C#'s syntax
  * Custom methods (?)
  * Custom types (structs) (?)
* Document
* Write more Tests
* Add debugging support


## Source Guide

### Demo
[RediSharp.Demo](./tests/RediSharp.Demo) is a demo project that utilizes the library.  

### C# Code Decompilation
RediSharp uses [ILSpy](https://github.com/icsharpcode/ILSpy) to decompile compiled lambda function to a C# syntax tree.  
See the [CSharp](./src/RediSharp/CSharp) folder. 

### RedIL
[RedIL](./src/RediSharp/RedIL) is an intermidiate language that is created from C# code, and later compiled to Lua, and potentially other targets in the future (See "Future Plans").  

### Lua Compilation
Lua is written by traversing the RedIL using an [IRedILVisitor](./src/RediSharp/RedIL/IRedILVisitor.cs).  
See the [Lua](./src/RediSharp/Lua) folder. 

## Dependencies
* [LiveDelegate.ILSpy](https://github.com/areller/LiveDelegate) - MIT License (Uses ILSpy to decompile compiled delegates)
* [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis) - MIT License
* [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) - MIT License

## Future Plans

* Transpiling C# Code directly to C code that will run as a Redis module.

## Disclaimer
This library is still at it's early stage of development and not meant to be production ready yet. Use at your own risk.
