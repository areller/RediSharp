using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using RediSharp.Enums;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Extensions;
using RediSharp.RedIL.Nodes;
using RediSharp.RedIL.Resolving;
using RediSharp.RedIL.Resolving.Attributes;
using StackExchange.Redis;

namespace RediSharp.Lib.Internal.Types
{
    class DatabaseResolverPack
    {
        #region Common Dictionaries

        private static readonly DictionaryTableDefinitionNode _zsetRangeByRankOrder = new DictionaryTableDefinitionNode(
            new[]
            {
                new KeyValuePair<ExpressionNode, ExpressionNode>((ConstantValueNode) 0, (ConstantValueNode) "ZRANGE"),
                new KeyValuePair<ExpressionNode, ExpressionNode>((ConstantValueNode) 1,
                    (ConstantValueNode) "ZREVRANGE"),
            });

        private static readonly DictionaryTableDefinitionNode _zsetRangeByScoreOrder =
            new DictionaryTableDefinitionNode(new[]
            {
                new KeyValuePair<ExpressionNode, ExpressionNode>((ConstantValueNode) 0,
                    (ConstantValueNode) "ZRANGEBYSCORE"),
                new KeyValuePair<ExpressionNode, ExpressionNode>((ConstantValueNode) 1,
                    (ConstantValueNode) "ZREVRANGEBYSCORE")
            });

        private static readonly DictionaryTableDefinitionNode _zSetCombineDict = new DictionaryTableDefinitionNode(new[]
        {
            new KeyValuePair<ExpressionNode, ExpressionNode>((ConstantValueNode) 0,
                (ConstantValueNode) "ZUNIONSTORE"),
            new KeyValuePair<ExpressionNode, ExpressionNode>((ConstantValueNode) 1,
                (ConstantValueNode) "ZINTERSTORE")
        });

        private static readonly DictionaryTableDefinitionNode _zsetStartExclusive = new DictionaryTableDefinitionNode(
            new[]
            {
                new KeyValuePair<ExpressionNode, ExpressionNode>((ConstantValueNode) 0, ExpressionNode.Empty),
                new KeyValuePair<ExpressionNode, ExpressionNode>((ConstantValueNode) 1, (ConstantValueNode) "("),
                new KeyValuePair<ExpressionNode, ExpressionNode>((ConstantValueNode) 2, ExpressionNode.Empty),
                new KeyValuePair<ExpressionNode, ExpressionNode>((ConstantValueNode) 3, (ConstantValueNode) "("),
            });

        private static readonly DictionaryTableDefinitionNode _zsetEndExclusive = new DictionaryTableDefinitionNode(
            new[]
            {
                new KeyValuePair<ExpressionNode, ExpressionNode>((ConstantValueNode) 0, ExpressionNode.Empty),
                new KeyValuePair<ExpressionNode, ExpressionNode>((ConstantValueNode) 1, ExpressionNode.Empty),
                new KeyValuePair<ExpressionNode, ExpressionNode>((ConstantValueNode) 2, (ConstantValueNode) "("),
                new KeyValuePair<ExpressionNode, ExpressionNode>((ConstantValueNode) 3, (ConstantValueNode) "("),
            });

        private static readonly DictionaryTableDefinitionNode _zsetStartLexExclusive =
            new DictionaryTableDefinitionNode(new[]
            {
                new KeyValuePair<ExpressionNode, ExpressionNode>((ConstantValueNode) 0, (ConstantValueNode) "["),
                new KeyValuePair<ExpressionNode, ExpressionNode>((ConstantValueNode) 1, (ConstantValueNode) "("),
                new KeyValuePair<ExpressionNode, ExpressionNode>((ConstantValueNode) 2, (ConstantValueNode) "["),
                new KeyValuePair<ExpressionNode, ExpressionNode>((ConstantValueNode) 3, (ConstantValueNode) "("),
            });

        private static readonly DictionaryTableDefinitionNode _zsetEndLexExclusive = new DictionaryTableDefinitionNode(
            new[]
            {
                new KeyValuePair<ExpressionNode, ExpressionNode>((ConstantValueNode) 0, (ConstantValueNode) "["),
                new KeyValuePair<ExpressionNode, ExpressionNode>((ConstantValueNode) 1, (ConstantValueNode) "["),
                new KeyValuePair<ExpressionNode, ExpressionNode>((ConstantValueNode) 2, (ConstantValueNode) "("),
                new KeyValuePair<ExpressionNode, ExpressionNode>((ConstantValueNode) 3, (ConstantValueNode) "("),
            });
        
        #endregion
        
        abstract class RedisMethodResolver : RedILMethodResolver
        {
            private static ExpressionNode[] _empty = new ExpressionNode[0];

            protected ExpressionNode FormatStringArgument(ExpressionNode arg) => arg.AsString();
            
            protected IEnumerable<ExpressionNode> EvaluateArray(ExpressionNode node, bool allKeys)
            {
                if (node is null)
                {
                    return _empty;
                }
                
                if (node.DataType == DataValueType.Array)
                {
                    if (node.Type == RedILNodeType.ArrayTableDefinition)
                    {
                        var res = (node as ArrayTableDefinitionNode).Elements;
                        if (!allKeys)
                        {
                            return res.Select(FormatStringArgument);
                        }

                        return res;
                    }
                    else
                    {
                        if (!allKeys)
                        {
                            return WrapSingle(new CallLuaFunctionNode(LuaFunction.TableUnpack, DataValueType.Array,
                                WrapSingle(node)));
                        }
                        else
                        {
                            return WrapSingle(new CallBuiltinLuaMethodNode(LuaBuiltinMethod.TableUnpack,
                                WrapSingle(node)));
                        }
                    }
                }

                return WrapSingle(allKeys ? node : FormatStringArgument(node));
            }

            protected IEnumerable<ExpressionNode> EvaluateKVArray(ExpressionNode node)
            {
                if (node is null)
                {
                    return _empty;
                }

                if (node.DataType == DataValueType.Array)
                {
                    if (node.Type == RedILNodeType.ArrayTableDefinition &&
                        ((ArrayTableDefinitionNode) node).Elements.All(elem =>
                            elem.Type == RedILNodeType.ArrayTableDefinition))
                    {
                        return (node as ArrayTableDefinitionNode).Elements.SelectMany(elem =>
                            (elem as ArrayTableDefinitionNode).Elements).Select(FormatStringArgument);
                    }
                    else
                    {
                        return WrapSingle(new CallLuaFunctionNode(LuaFunction.TableDeepUnpack, DataValueType.Array,
                            WrapSingle(node)));
                    }
                }

                return WrapSingle(FormatStringArgument(node));
            }

            protected ExpressionNode ArrayLength(ExpressionNode node)
            {
                if (node is null)
                {
                    return (ConstantValueNode) 0;
                }

                if (node.Type == RedILNodeType.ArrayTableDefinition)
                {
                    return (ConstantValueNode) ((ArrayTableDefinitionNode) node).Elements.Count;
                }

                return new CallBuiltinLuaMethodNode(LuaBuiltinMethod.TableGetN, new[] {node});
            }
            
            protected ExpressionNode[] WrapSingle(ExpressionNode node) => new ExpressionNode[] {node};
        }

        class SimpleRedisMethodResolver : RedisMethodResolver
        {
            private string _name;

            private DataValueType _dataType;

            private bool _allKeys;

            private int? _numArgs;

            public SimpleRedisMethodResolver(object arg1, object arg2, object arg3)
            {
                _name = (string) arg1;
                _dataType = (DataValueType) arg2;
                _allKeys = (bool) arg3;
            }

            public SimpleRedisMethodResolver(object arg1, object arg2, object arg3, object arg4)
                : this(arg1, arg2, arg3)
            {
                _numArgs = (int) arg4;
            }

            public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
            {
                IEnumerable<ExpressionNode> args = arguments;
                if (_numArgs.HasValue)
                {
                    args = args.Take(_numArgs.Value);
                }

                return new CallRedisMethodNode(_name, _dataType, caller,
                    args.SelectMany(arg => EvaluateArray(arg, _allKeys)).ToList());
            }
        }

        class StringSetResolver : RedisMethodResolver
        {
            public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
            {
                return context.Compiler.IfTable(context, DataValueType.Boolean, new[]
                {
                    new KeyValuePair<ExpressionNode, ExpressionNode>(
                        arguments.At(2).IsNil() && arguments.At(3).IsNilOrEmpty(), new CallRedisMethodNode("SET",
                            DataValueType.Boolean, caller,
                            new List<ExpressionNode>() {arguments.At(0).AsString(), arguments.At(1).AsString()})),
                    new KeyValuePair<ExpressionNode, ExpressionNode>(
                        !arguments.At(2).IsNil() && arguments.At(3).IsNilOrEmpty(), new CallRedisMethodNode("SET",
                            DataValueType.Boolean, caller,
                            new List<ExpressionNode>()
                                {arguments.At(0).AsString(), arguments.At(1).AsString(), (ConstantValueNode) "PX", arguments.At(2)})),
                    new KeyValuePair<ExpressionNode, ExpressionNode>(
                        arguments.At(2).IsNil() && !arguments.At(3).IsNilOrEmpty(),
                        new CallRedisMethodNode("SET", DataValueType.Boolean, caller,
                            new List<ExpressionNode>() {arguments.At(0).AsString(), arguments.At(1).AsString(), arguments.At(3)})),
                    new KeyValuePair<ExpressionNode, ExpressionNode>(
                        !arguments.At(2).IsNil() && !arguments.At(3).IsNilOrEmpty(),
                        new CallRedisMethodNode("SET", DataValueType.Boolean, caller,
                            new List<ExpressionNode>()
                            {
                                arguments.At(0).AsString(), arguments.At(1).AsString(), (ConstantValueNode) "PX",
                                arguments.At(2), arguments.At(3)
                            }))
                });
            }
        }
        
        class StringSetManyResolver : RedisMethodResolver
        {
            public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
            {
                return context.Compiler.IfTable(context, DataValueType.Boolean, new[]
                {
                    new KeyValuePair<ExpressionNode, ExpressionNode>(arguments.At(1) != (ConstantValueNode) "NX",
                        new CallRedisMethodNode("MSET", DataValueType.Boolean, caller,
                            EvaluateKVArray(arguments.At(0)).ToList())),
                    new KeyValuePair<ExpressionNode, ExpressionNode>(arguments.At(1) == (ConstantValueNode) "NX",
                        new CallRedisMethodNode("MSETNX", DataValueType.Boolean, caller,
                            EvaluateKVArray(arguments.At(0)).ToList())),
                });
            }
        }

        class IncrByResolver : RedisMethodResolver
        {
            private bool _isFloat;

            private bool _isNegative;
            
            public IncrByResolver(object arg1, object arg2)
            {
                _isFloat = (bool) arg1;
                _isNegative = (bool) arg2;
            }
            
            public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
            {
                return new CallRedisMethodNode(_isFloat ? "INCRBYFLOAT" : "INCRBY", DataValueType.Float, caller,
                    new[]
                    {
                        arguments.At(0),
                        _isNegative
                            ? UnaryExpressionNode.Create(UnaryExpressionOperator.Minus, arguments.At(1))
                            : arguments.At(1)
                    });
            }
        }

        class HashIncrByResolver : RedisMethodResolver
        {
            private bool _isFloat;

            private bool _isNegative;

            public HashIncrByResolver(object arg1, object arg2)
            {
                _isFloat = (bool) arg1;
                _isNegative = (bool) arg2;
            }
            
            public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
            {
                return new CallRedisMethodNode(_isFloat ? "HINCRBYFLOAT" : "HINCRBY", DataValueType.Float, caller,
                    new[]
                    {
                        arguments.At(0), arguments.At(1).AsString(),
                        _isNegative
                            ? UnaryExpressionNode.Create(UnaryExpressionOperator.Minus, arguments.At(2))
                            : arguments.At(2)
                    });
            }
        }

        class ListSinglePushResolver : RedisMethodResolver
        {
            private string _pushCmd;

            private string _pushxCmd;
            
            public ListSinglePushResolver(object arg)
            {
                int leftOrRight = (int) arg;
                if (leftOrRight == 0)
                {
                    _pushCmd = "RPUSH";
                    _pushxCmd = "RPUSHX";
                }
                else
                {
                    _pushCmd = "LPUSH";
                    _pushxCmd = "LPUSHX";
                }
            }
            
            public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
            {
                return context.Compiler.IfTable(context, DataValueType.Integer, new[]
                {
                    new KeyValuePair<ExpressionNode, ExpressionNode>(arguments.At(2) != (ConstantValueNode) "XX",
                        new CallRedisMethodNode(_pushCmd, DataValueType.Integer, caller,
                            new[] {arguments.At(0), arguments.At(1).AsString()})),
                    new KeyValuePair<ExpressionNode, ExpressionNode>(arguments.At(2) == (ConstantValueNode) "XX",
                        new CallRedisMethodNode(_pushxCmd, DataValueType.Integer, caller,
                            new[] {arguments.At(0), arguments.At(1).AsString()})),
                });
            }
        }

        class ListRemoveResolver : RedisMethodResolver
        {
            public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
            {
                return new CallRedisMethodNode("LREM", DataValueType.Integer, caller,
                    new[] {arguments.At(0), arguments.At(2), arguments.At(1).AsString()});
            }
        }
        
        class ListInsertResolver : RedisMethodResolver
        {
            private string _beforeAfter;
            
            public ListInsertResolver(object arg)
            {
                _beforeAfter = (string) arg;
            }
            
            public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
            {
                return new CallRedisMethodNode("LINSERT", DataValueType.Integer, caller,
                    new[]
                    {
                        arguments.At(0), (ConstantValueNode) _beforeAfter, arguments.At(1).AsString(),
                        arguments.At(2).AsString()
                    });
            }
        }

        class HashSetResolver : RedisMethodResolver
        {
            public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
            {
                return context.Compiler.IfTable(context, DataValueType.Boolean, new[]
                {
                    new KeyValuePair<ExpressionNode, ExpressionNode>(arguments.At(3) != (ConstantValueNode) "NX",
                        new CallRedisMethodNode("HSET", DataValueType.Boolean, caller,
                            new[] {arguments.At(0), arguments.At(1).AsString(), arguments.At(2).AsString()})),
                    new KeyValuePair<ExpressionNode, ExpressionNode>(arguments.At(3) == (ConstantValueNode) "NX",
                        new CallRedisMethodNode("HSETNX", DataValueType.Boolean, caller,
                            new[] {arguments.At(0), arguments.At(1).AsString(), arguments.At(2).AsString()})),
                });
            }
        }

        class HashSetManyResolver : RedisMethodResolver
        {
            public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
            {
                return new CallRedisMethodNode("HMSET", DataValueType.Unknown, caller,
                    new[] {arguments.At(0)}.Concat(EvaluateKVArray(arguments.At(1))).ToList());
            }
        }

        class HashGetAllResolver : RedisMethodResolver
        {
            public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
            {
                return new CallLuaFunctionNode(LuaFunction.TableGroupToKV, DataValueType.Array,
                    new[] {new CallRedisMethodNode("HGETALL", DataValueType.Array, caller, new[] {arguments.At(0)})});
            }
        }

        class SetCombineResolver : RedisMethodResolver
        {
            public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
            {
                var args = arguments.Length == 4
                    ? new[] {arguments.At(1), arguments.At(2)}
                    : EvaluateArray(arguments.At(1), true);
                return context.Compiler.IfTable(context, DataValueType.Array, new[]
                {
                    new KeyValuePair<ExpressionNode, ExpressionNode>(arguments.At(0) == (ConstantValueNode) 0,
                        new CallRedisMethodNode("SUNION", DataValueType.Array, caller, args.ToArray())),
                    new KeyValuePair<ExpressionNode, ExpressionNode>(arguments.At(0) == (ConstantValueNode) 1,
                        new CallRedisMethodNode("SINTER", DataValueType.Array, caller, args.ToArray())),
                    new KeyValuePair<ExpressionNode, ExpressionNode>(arguments.At(0) == (ConstantValueNode) 2,
                        new CallRedisMethodNode("SDIFF", DataValueType.Array, caller, args.ToArray())),
                });
            }
        }

        class SetCombineAndStoreResolver : RedisMethodResolver
        {
            public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
            {
                var args = arguments.Length == 5
                    ? new[] {arguments.At(1), arguments.At(2), arguments.At(3)}
                    : new[] {arguments.At(1)}.Concat(EvaluateArray(arguments.At(2), true));
                return context.Compiler.IfTable(context, DataValueType.Integer, new[]
                {
                    new KeyValuePair<ExpressionNode, ExpressionNode>(arguments.At(0) == (ConstantValueNode) 0,
                        new CallRedisMethodNode("SUNIONSTORE", DataValueType.Integer, caller, args.ToArray())),
                    new KeyValuePair<ExpressionNode, ExpressionNode>(arguments.At(0) == (ConstantValueNode) 1,
                        new CallRedisMethodNode("SINTERSTORE", DataValueType.Integer, caller, args.ToArray())),
                    new KeyValuePair<ExpressionNode, ExpressionNode>(arguments.At(0) == (ConstantValueNode) 2,
                        new CallRedisMethodNode("SDIFFSTORE", DataValueType.Integer, caller, args.ToArray())),
                });
            }
        }

        class SortedSetAddResolver : RedisMethodResolver
        {
            public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
            {
                if (arguments.Length == 4)
                {
                    return new CallRedisMethodNode("ZADD", DataValueType.Boolean, caller,
                        new[] {arguments.At(0), arguments.At(2), arguments.At(1).AsString()});
                }
                else if (arguments.Length == 5)
                {
                    return context.Compiler.IfTable(context, DataValueType.Boolean, new[]
                    {
                        new KeyValuePair<ExpressionNode, ExpressionNode>(arguments.At(3) == ExpressionNode.Empty,
                            new CallRedisMethodNode("ZADD", DataValueType.Boolean, caller,
                                new[] {arguments.At(0), arguments.At(2), arguments.At(1).AsString()})),
                        new KeyValuePair<ExpressionNode, ExpressionNode>(arguments.At(3) != ExpressionNode.Empty,
                            new CallRedisMethodNode("ZADD", DataValueType.Boolean, caller,
                                new[] {arguments.At(0), arguments.At(3), arguments.At(2), arguments.At(1).AsString()})),
                    });
                }
                
                throw new NotSupportedException();
            }
        }

        class SortedSetAddManyResolver : RedisMethodResolver
        {
            public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
            {
                if (arguments.Length == 3)
                {
                    return new CallRedisMethodNode("ZADD", DataValueType.Integer, caller,
                        new[] {arguments.At(0)}.Concat(EvaluateKVArray(arguments.At(1))).ToArray());
                }
                else if (arguments.Length == 4)
                {
                    return context.Compiler.IfTable(context, DataValueType.Integer, new[]
                    {
                        new KeyValuePair<ExpressionNode, ExpressionNode>(arguments.At(2) == ExpressionNode.Empty,
                            new CallRedisMethodNode("ZADD", DataValueType.Integer, caller,
                                new[] {arguments.At(0)}.Concat(EvaluateKVArray(arguments.At(1))).ToArray())),
                        new KeyValuePair<ExpressionNode, ExpressionNode>(arguments.At(2) != ExpressionNode.Empty,
                            new CallRedisMethodNode("ZADD", DataValueType.Integer, caller,
                                new[] {arguments.At(0), arguments.At(2)}.Concat(EvaluateKVArray(arguments.At(1)))
                                    .ToArray())),
                    });
                }
                
                throw new NotSupportedException();
            }
        }

        class SortedSetCombineAndStoreResolver : RedisMethodResolver
        {
            public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
            {
                //TOOD: Check how to implement DIFFSTORE
                return new CallRedisMethodNode(
                    context.Compiler.Dictionary(context, nameof(_zSetCombineDict), _zSetCombineDict, arguments.At(0)), DataValueType.Integer, caller, new[]
                    {
                        arguments.At(1), (ConstantValueNode) 2, arguments.At(2), arguments.At(3),
                        (ConstantValueNode) "AGGREGATE", arguments.At(4)
                    });
            }
        }

        class SortedSetCombineAndStoreManyResolver : RedisMethodResolver
        {
            public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
            {
                return context.Compiler.IfTable(context, DataValueType.Integer, new[]
                {
                    new KeyValuePair<ExpressionNode, ExpressionNode>(arguments.At(3) == ExpressionNode.Nil,
                        new CallRedisMethodNode(
                            context.Compiler.Dictionary(context, nameof(_zSetCombineDict), _zSetCombineDict,
                                arguments.At(0)), DataValueType.Integer, caller,
                            new[] {arguments.At(1), ArrayLength(arguments.At(2))}
                                .Concat(EvaluateArray(arguments.At(2), true))
                                .Concat(new[] {(ConstantValueNode) "AGGREGATE", arguments.At(4)})
                                .ToList())),
                    new KeyValuePair<ExpressionNode, ExpressionNode>(arguments.At(3) != ExpressionNode.Nil,
                        new CallRedisMethodNode(
                            context.Compiler.Dictionary(context, nameof(_zSetCombineDict), _zSetCombineDict,
                                arguments.At(0)), DataValueType.Integer, caller,
                            new[] {arguments.At(1), ArrayLength(arguments.At(2))}
                                .Concat(EvaluateArray(arguments.At(2), true))
                                .Concat(new[]
                                    {(ConstantValueNode) "AGGREGATE", arguments.At(4)})
                                .Concat(new[] {(ConstantValueNode) "WEIGHTS"})
                                .Concat(EvaluateArray(arguments.At(3), true)).ToList()))
                });
            }
        }

        class SortedSetIncrementResolver : RedisMethodResolver
        {
            private bool _positive;

            public SortedSetIncrementResolver(object arg)
            {
                _positive = (bool) arg;
            }

            public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
            {
                return new CallRedisMethodNode("ZINCRBY", DataValueType.Float, caller,
                    new[]
                    {
                        arguments.At(0),
                        _positive
                            ? arguments.At(2)
                            : UnaryExpressionNode.Create(UnaryExpressionOperator.Minus, arguments.At(2)),
                        arguments.At(1).AsString()
                    });
            }
        }

        class SortedSetLengthResolver : RedisMethodResolver
        {
            public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
            {
                return context.Compiler.IfTable(context, DataValueType.Integer, new[]
                {
                    new KeyValuePair<ExpressionNode, ExpressionNode>(
                        arguments.At(1) == (ConstantValueNode) "-inf" && arguments.At(2) == (ConstantValueNode) "+inf",
                        new CallRedisMethodNode("ZCARD", DataValueType.Integer, caller, new[] {arguments.At(0)})),
                    new KeyValuePair<ExpressionNode, ExpressionNode>(
                        arguments.At(1) != (ConstantValueNode) "-inf" || arguments.At(2) != (ConstantValueNode) "+inf",
                        new CallRedisMethodNode("ZCOUNT", DataValueType.Integer, caller, new[]
                        {
                            arguments.At(0),
                            BinaryExpressionNode.Create(BinaryExpressionOperator.StringConcat,
                                context.Compiler.Dictionary(context, nameof(_zsetStartExclusive), _zsetStartExclusive,
                                    arguments.At(3)), arguments.At(1)),
                            BinaryExpressionNode.Create(BinaryExpressionOperator.StringConcat,
                                context.Compiler.Dictionary(context, nameof(_zsetEndExclusive), _zsetEndExclusive,
                                    arguments.At(3)), arguments.At(2))
                        }))
                });
            }
        }

        class SortedSetLengthByValueResolver : RedisMethodResolver
        {
            public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
            {
                return context.Compiler.IfTable(context, DataValueType.Integer, new[]
                {
                    new KeyValuePair<ExpressionNode, ExpressionNode>(
                        arguments.At(1) == (ConstantValueNode) "-" && arguments.At(2) == (ConstantValueNode) "+",
                        new CallRedisMethodNode("ZLEXCOUNT", DataValueType.Integer, caller,
                            new[] {arguments.At(0), (ConstantValueNode) "-", (ConstantValueNode) "+"})),
                    new KeyValuePair<ExpressionNode, ExpressionNode>(
                        arguments.At(1) == (ConstantValueNode) "-" && arguments.At(2) != (ConstantValueNode) "+",
                        new CallRedisMethodNode("ZLEXCOUNT", DataValueType.Integer, caller,
                            new[]
                            {
                                arguments.At(0), (ConstantValueNode) "-",
                                BinaryExpressionNode.Create(BinaryExpressionOperator.StringConcat,
                                    context.Compiler.Dictionary(context, nameof(_zsetEndLexExclusive),
                                        _zsetEndLexExclusive, arguments.At(3)), arguments.At(2).AsString())
                            })),
                    new KeyValuePair<ExpressionNode, ExpressionNode>(
                        arguments.At(1) != (ConstantValueNode) "-" && arguments.At(2) == (ConstantValueNode) "+",
                        new CallRedisMethodNode("ZLEXCOUNT", DataValueType.Integer, caller,
                            new[]
                            {
                                arguments.At(0),
                                BinaryExpressionNode.Create(BinaryExpressionOperator.StringConcat,
                                    context.Compiler.Dictionary(context, nameof(_zsetStartLexExclusive),
                                        _zsetStartLexExclusive, arguments.At(3)), arguments.At(1).AsString()),
                                (ConstantValueNode) "+"
                            })),
                    new KeyValuePair<ExpressionNode, ExpressionNode>(
                        arguments.At(1) != (ConstantValueNode) "-" && arguments.At(2) != (ConstantValueNode) "+",
                        new CallRedisMethodNode("ZLEXCOUNT", DataValueType.Integer, caller,
                            new[]
                            {
                                arguments.At(0),
                                BinaryExpressionNode.Create(BinaryExpressionOperator.StringConcat,
                                    context.Compiler.Dictionary(context, nameof(_zsetStartLexExclusive),
                                        _zsetStartLexExclusive, arguments.At(3)), arguments.At(1).AsString()),
                                BinaryExpressionNode.Create(BinaryExpressionOperator.StringConcat,
                                    context.Compiler.Dictionary(context, nameof(_zsetEndLexExclusive),
                                        _zsetEndLexExclusive, arguments.At(3)), arguments.At(2).AsString())
                            }))
                });
            }
        }

        class SortedSetRangeByRankResolver : RedisMethodResolver
        {
            private bool _withScores;

            public SortedSetRangeByRankResolver(object arg)
            {
                _withScores = (bool) arg;
            }
            
            public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
            {
                var args = new List<ExpressionNode>() {arguments.At(0), arguments.At(1), arguments.At(2)};
                if (_withScores)
                {
                    args.Add((ConstantValueNode) "WITHSCORES");
                }

                var callRedis = new CallRedisMethodNode(
                    context.Compiler.Dictionary(context, nameof(_zsetRangeByRankOrder), _zsetRangeByRankOrder,
                        arguments.At(3)), DataValueType.Array, caller, args);

                if (_withScores)
                {
                    return new CallLuaFunctionNode(LuaFunction.TableGroupToKVReverse, DataValueType.Array,
                        new[] {callRedis});
                }

                return callRedis;
            }
        }

        class SortedSetRangeByScoreResolver : RedisMethodResolver
        {
            private bool _withScores;

            public SortedSetRangeByScoreResolver(object arg)
            {
                _withScores = (bool) arg;
            }
            
            public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
            {
                var args = new List<ExpressionNode>()
                {
                    arguments.At(0),
                    BinaryExpressionNode.Create(BinaryExpressionOperator.StringConcat,
                        context.Compiler.Dictionary(context, nameof(_zsetStartExclusive), _zsetStartExclusive,
                            arguments.At(3)), arguments.At(1)),
                    BinaryExpressionNode.Create(BinaryExpressionOperator.StringConcat,
                        context.Compiler.Dictionary(context, nameof(_zsetEndExclusive), _zsetEndExclusive,
                            arguments.At(3)), arguments.At(2))
                };
                if (_withScores)
                {
                    args.Add((ConstantValueNode) "WITHSCORES");
                }
                args.Add((ConstantValueNode)"LIMIT");
                args.Add(arguments.At(5));
                args.Add(arguments.At(6));

                var callRedis = new CallRedisMethodNode(
                    context.Compiler.Dictionary(context, nameof(_zsetRangeByScoreOrder), _zsetRangeByScoreOrder,
                        arguments.At(4)), DataValueType.Array, caller, args);

                if (_withScores)
                {
                    return new CallLuaFunctionNode(LuaFunction.TableGroupToKVReverse, DataValueType.Array,
                        new[] {callRedis});
                }
                
                return callRedis;
            }
        }

        class DatabaseProxy : IDatabase
        {
            #region Strings
            
            //GET key
            [RedILResolve(typeof(SimpleRedisMethodResolver), "GET", DataValueType.String, true, 1)]
            public RedisValue StringGet(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            //MGET key1 key2 ...
            [RedILResolve(typeof(SimpleRedisMethodResolver), "MGET", DataValueType.Array, true, 1)]
            public RedisValue[] StringGet(RedisKey[] keys, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }
            
            //SET key value [exp]
            [RedILResolve(typeof(StringSetResolver))]
            public bool StringSet(RedisKey key, RedisValue value, TimeSpan? expiry = null, When when = When.Always, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            //MSET key1 val1 key2 val2
            [RedILResolve(typeof(StringSetManyResolver))]
            public bool StringSet(KeyValuePair<RedisKey, RedisValue>[] values, When when = When.Always, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }
            
            //INCRBY key val
            [RedILResolve(typeof(IncrByResolver), false, false)]
            public long StringIncrement(RedisKey key, long value = 1, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            //INCRBYFLOAT key val
            [RedILResolve(typeof(IncrByResolver), true, false)]
            public double StringIncrement(RedisKey key, double value, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            //STRLEN key
            [RedILResolve(typeof(SimpleRedisMethodResolver), "STRLEN", DataValueType.Integer, true, 1)]
            public long StringLength(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }
            
            //GETSET key value
            [RedILResolve(typeof(SimpleRedisMethodResolver), "GETSET", DataValueType.String, false, 2)]
            public RedisValue StringGetSet(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            //SETRANGE key offset value
            [RedILResolve(typeof(SimpleRedisMethodResolver), "SETRANGE", DataValueType.Integer, false, 3)]
            public RedisValue StringSetRange(RedisKey key, long offset, RedisValue value, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            //GETRANGE key start end
            [RedILResolve(typeof(SimpleRedisMethodResolver), "GETRANGE", DataValueType.String, false, 3)]
            public RedisValue StringGetRange(RedisKey key, long start, long end, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }
            
            //INCRBY key -value
            [RedILResolve(typeof(IncrByResolver), false, true)]
            public long StringDecrement(RedisKey key, long value = 1, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            //INCRBYFLOAT key -value
            [RedILResolve(typeof(IncrByResolver), true, true)]
            public double StringDecrement(RedisKey key, double value, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }
            
            #endregion
            
            #region Lists
            
            //LTRIM key start stop
            [RedILResolve(typeof(SimpleRedisMethodResolver), "LTRIM", DataValueType.Unknown, false, 3)]
            public void ListTrim(RedisKey key, long start, long stop, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }
            
            //LSET key index value
            [RedILResolve(typeof(SimpleRedisMethodResolver), "LSET", DataValueType.Unknown, false, 3)]
            public void ListSetByIndex(RedisKey key, long index, RedisValue value, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }
            
            //RPUSH key value
            [RedILResolve(typeof(ListSinglePushResolver), 0)]
            public long ListRightPush(RedisKey key, RedisValue value, When when = When.Always, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            //RPUSH key value1 value2 ...
            [RedILResolve(typeof(SimpleRedisMethodResolver), "RPUSH", DataValueType.Integer, false, 2)]
            public long ListRightPush(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }
            
            //RPOPLPUSH source dest
            [RedILResolve(typeof(SimpleRedisMethodResolver), "RPOPLPUSH", DataValueType.String, true, 2)]
            public RedisValue ListRightPopLeftPush(RedisKey source, RedisKey destination, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }
            
            //RPOP key
            [RedILResolve(typeof(SimpleRedisMethodResolver), "RPOP", DataValueType.String, true, 1)]
            public RedisValue ListRightPop(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }
            
            //LREM key count value
            [RedILResolve(typeof(ListRemoveResolver))]
            public long ListRemove(RedisKey key, RedisValue value, long count = 0, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }
            
            //LRANGE key start stop
            [RedILResolve(typeof(SimpleRedisMethodResolver), "LRANGE", DataValueType.Array, false, 3)]
            public RedisValue[] ListRange(RedisKey key, long start = 0, long stop = -1, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }
            
            //LLEN key
            [RedILResolve(typeof(SimpleRedisMethodResolver), "LLEN", DataValueType.Integer, true, 1)]
            public long ListLength(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }
            
            //LPOP key
            [RedILResolve(typeof(SimpleRedisMethodResolver), "LPOP", DataValueType.String, true, 1)]
            public RedisValue ListLeftPop(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            //LPUSH key value
            [RedILResolve(typeof(ListSinglePushResolver), 1)]
            public long ListLeftPush(RedisKey key, RedisValue value, When when = When.Always, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            //LPUSH key value1 value2 ...
            [RedILResolve(typeof(SimpleRedisMethodResolver), "LPUSH", DataValueType.Integer, false, 2)]
            public long ListLeftPush(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }
            
            //LINSERT key AFTER pivot value
            [RedILResolve(typeof(ListInsertResolver), "AFTER")]
            public long ListInsertAfter(RedisKey key, RedisValue pivot, RedisValue value, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            //LINSERT key BEFORE pivot value
            [RedILResolve(typeof(ListInsertResolver), "BEFORE")]
            public long ListInsertBefore(RedisKey key, RedisValue pivot, RedisValue value, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }
            
            //LINDEX key index
            [RedILResolve(typeof(SimpleRedisMethodResolver), "LINDEX", DataValueType.String, false, 2)]
            public RedisValue ListGetByIndex(RedisKey key, long index, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }
            
            #endregion
            
            #region Hash
            
            //HVALS key
            [RedILResolve(typeof(SimpleRedisMethodResolver), "HVALS", DataValueType.Array, true, 1)]
            public RedisValue[] HashValues(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }
            
            //HSET key hashField value
            [RedILResolve(typeof(HashSetResolver))]
            public bool HashSet(RedisKey key, RedisValue hashField, RedisValue value, When when = When.Always, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }
            
            //HSET key field1 val1 field2 val2 ...
            [RedILResolve(typeof(HashSetManyResolver))]
            public void HashSet(RedisKey key, HashEntry[] hashFields, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }
            
            //HLEN key
            [RedILResolve(typeof(SimpleRedisMethodResolver), "HLEN", DataValueType.Integer, true, 1)]
            public long HashLength(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }
            
            //HKEYS key
            [RedILResolve(typeof(SimpleRedisMethodResolver), "HKEYS", DataValueType.Array, true, 1)]
            public RedisValue[] HashKeys(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }
            
            //HINCRBY key hashField value
            [RedILResolve(typeof(HashIncrByResolver), false, false)]
            public long HashIncrement(RedisKey key, RedisValue hashField, long value = 1, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            //HINCRBYFLOAT key hashField value
            [RedILResolve(typeof(HashIncrByResolver), true, false)]
            public double HashIncrement(RedisKey key, RedisValue hashField, double value, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }
            
            //HGETALL key
            [RedILResolve(typeof(HashGetAllResolver))]
            public HashEntry[] HashGetAll(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }
            
            //HMGET key hashField1 hashField2 ...
            [RedILResolve(typeof(SimpleRedisMethodResolver), "HMGET", DataValueType.Array, false, 2)]
            public RedisValue[] HashGet(RedisKey key, RedisValue[] hashFields, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }
            
            //HDEL key hashField
            [RedILResolve(typeof(SimpleRedisMethodResolver), "HDEL", DataValueType.Boolean, false, 2)]
            public bool HashDelete(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            //HDEL key hashField1 hashField2 ...
            [RedILResolve(typeof(SimpleRedisMethodResolver), "HDEL", DataValueType.Integer, false, 2)]
            public long HashDelete(RedisKey key, RedisValue[] hashFields, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            //HEXISTS key hashField
            [RedILResolve(typeof(SimpleRedisMethodResolver), "HEXISTS", DataValueType.Boolean, false, 2)]
            public bool HashExists(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            //HGET key hashField
            [RedILResolve(typeof(SimpleRedisMethodResolver), "HGET", DataValueType.String, false, 2)]
            public RedisValue HashGet(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            //HINCYBY key hashField value
            [RedILResolve(typeof(HashIncrByResolver), false, true)]
            public long HashDecrement(RedisKey key, RedisValue hashField, long value = 1, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            //HINCRBYFLOAT key hashField value
            [RedILResolve(typeof(HashIncrByResolver), true, true)]
            public double HashDecrement(RedisKey key, RedisValue hashField, double value, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }
            
            #endregion
            
            #region Sets
            
            //SADD key value
            [RedILResolve(typeof(SimpleRedisMethodResolver), "SADD", DataValueType.Boolean, false, 2)]
            public bool SetAdd(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            //SADD key value1 value2 ...
            [RedILResolve(typeof(SimpleRedisMethodResolver), "SADD", DataValueType.Integer, false, 2)]
            public long SetAdd(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            [RedILResolve(typeof(SetCombineResolver))]
            public RedisValue[] SetCombine(SetOperation operation, RedisKey first, RedisKey second, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            [RedILResolve(typeof(SetCombineResolver))]
            public RedisValue[] SetCombine(SetOperation operation, RedisKey[] keys, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            [RedILResolve(typeof(SetCombineAndStoreResolver))]
            public long SetCombineAndStore(SetOperation operation, RedisKey destination, RedisKey first, RedisKey second,
                CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            [RedILResolve(typeof(SetCombineAndStoreResolver))]
            public long SetCombineAndStore(SetOperation operation, RedisKey destination, RedisKey[] keys, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            [RedILResolve(typeof(SimpleRedisMethodResolver), "SISMEMBER", DataValueType.Boolean, false, 2)]
            public bool SetContains(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            [RedILResolve(typeof(SimpleRedisMethodResolver), "SCARD", DataValueType.Integer, true, 1)]
            public long SetLength(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            [RedILResolve(typeof(SimpleRedisMethodResolver), "SMEMBERS", DataValueType.Array, true, 1)]
            public RedisValue[] SetMembers(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            [RedILResolve(typeof(SimpleRedisMethodResolver), "SMOVE", DataValueType.Boolean, false, 3)]
            public bool SetMove(RedisKey source, RedisKey destination, RedisValue value, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            [RedILResolve(typeof(SimpleRedisMethodResolver), "SPOP", DataValueType.String, true, 1)]
            public RedisValue SetPop(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            [RedILResolve(typeof(SimpleRedisMethodResolver), "SPOP", DataValueType.Array, false, 2)]
            public RedisValue[] SetPop(RedisKey key, long count, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            [RedILResolve(typeof(SimpleRedisMethodResolver), "SRANDMEMBER", DataValueType.String, true, 1)]
            public RedisValue SetRandomMember(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            [RedILResolve(typeof(SimpleRedisMethodResolver), "SRANDMEMBER", DataValueType.Array, false, 2)]
            public RedisValue[] SetRandomMembers(RedisKey key, long count, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            [RedILResolve(typeof(SimpleRedisMethodResolver), "SREM", DataValueType.Boolean, false, 2)]
            public bool SetRemove(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            [RedILResolve(typeof(SimpleRedisMethodResolver), "SREM", DataValueType.Integer, false, 2)]
            public long SetRemove(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }
            
            #endregion

            #region Sorted Sets

            [RedILResolve(typeof(SortedSetAddResolver))]
            public bool SortedSetAdd(RedisKey key, RedisValue member, double score, CommandFlags flags)
            {
                throw new NotImplementedException();
            }

            [RedILResolve(typeof(SortedSetAddResolver))]
            public bool SortedSetAdd(RedisKey key, RedisValue member, double score, When when = When.Always, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            [RedILResolve(typeof(SortedSetAddManyResolver))]
            public long SortedSetAdd(RedisKey key, SortedSetEntry[] values, CommandFlags flags)
            {
                throw new NotImplementedException();
            }

            [RedILResolve(typeof(SortedSetAddManyResolver))]
            public long SortedSetAdd(RedisKey key, SortedSetEntry[] values, When when = When.Always, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            [RedILResolve(typeof(SortedSetCombineAndStoreResolver))]
            public long SortedSetCombineAndStore(SetOperation operation, RedisKey destination, RedisKey first, RedisKey second,
                Aggregate aggregate = Aggregate.Sum, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            [RedILResolve(typeof(SortedSetCombineAndStoreManyResolver))]
            public long SortedSetCombineAndStore(SetOperation operation, RedisKey destination, RedisKey[] keys, double[] weights = null,
                Aggregate aggregate = Aggregate.Sum, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            [RedILResolve(typeof(SortedSetIncrementResolver), false)]
            public double SortedSetDecrement(RedisKey key, RedisValue member, double value, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            [RedILResolve(typeof(SortedSetIncrementResolver), true)]
            public double SortedSetIncrement(RedisKey key, RedisValue member, double value, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            [RedILResolve(typeof(SortedSetLengthResolver))]
            public long SortedSetLength(RedisKey key, double min = double.NegativeInfinity, double max = double.PositiveInfinity, Exclude exclude = Exclude.None,
                CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            [RedILResolve(typeof(SortedSetLengthByValueResolver))]
            public long SortedSetLengthByValue(RedisKey key, RedisValue min, RedisValue max, Exclude exclude = Exclude.None,
                CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            [RedILResolve(typeof(SortedSetRangeByRankResolver), false)]
            public RedisValue[] SortedSetRangeByRank(RedisKey key, long start = 0, long stop = -1, Order order = Order.Ascending,
                CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            [RedILResolve(typeof(SortedSetRangeByRankResolver), true)]
            public SortedSetEntry[] SortedSetRangeByRankWithScores(RedisKey key, long start = 0, long stop = -1, Order order = Order.Ascending,
                CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            [RedILResolve(typeof(SortedSetRangeByScoreResolver), false)]
            public RedisValue[] SortedSetRangeByScore(RedisKey key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity,
                Exclude exclude = Exclude.None, Order order = Order.Ascending, long skip = 0, long take = -1, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            [RedILResolve(typeof(SortedSetRangeByScoreResolver), true)]
            public SortedSetEntry[] SortedSetRangeByScoreWithScores(RedisKey key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity,
                Exclude exclude = Exclude.None, Order order = Order.Ascending, long skip = 0, long take = -1, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public RedisValue[] SortedSetRangeByValue(RedisKey key, RedisValue min, RedisValue max, Exclude exclude, long skip,
                long take = -1, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public RedisValue[] SortedSetRangeByValue(RedisKey key, RedisValue min = new RedisValue(), RedisValue max = new RedisValue(),
                Exclude exclude = Exclude.None, Order order = Order.Ascending, long skip = 0, long take = -1, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public long? SortedSetRank(RedisKey key, RedisValue member, Order order = Order.Ascending, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public bool SortedSetRemove(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public long SortedSetRemove(RedisKey key, RedisValue[] members, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public long SortedSetRemoveRangeByRank(RedisKey key, long start, long stop, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public long SortedSetRemoveRangeByScore(RedisKey key, double start, double stop, Exclude exclude = Exclude.None,
                CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public long SortedSetRemoveRangeByValue(RedisKey key, RedisValue min, RedisValue max, Exclude exclude = Exclude.None,
                CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public double? SortedSetScore(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public SortedSetEntry? SortedSetPop(RedisKey key, Order order = Order.Ascending, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public SortedSetEntry[] SortedSetPop(RedisKey key, long count, Order order = Order.Ascending, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            #endregion
         
            #region Keys
            
            
            
            #endregion
            
            #region Unused
            
            public Task<TimeSpan> PingAsync(CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public bool TryWait(Task task)
            {
                throw new NotImplementedException();
            }

            public void Wait(Task task)
            {
                throw new NotImplementedException();
            }

            public T Wait<T>(Task<T> task)
            {
                throw new NotImplementedException();
            }

            public void WaitAll(params Task[] tasks)
            {
                throw new NotImplementedException();
            }

            public IConnectionMultiplexer Multiplexer { get; }
            public TimeSpan Ping(CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public bool IsConnected(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task KeyMigrateAsync(RedisKey key, EndPoint toServer, int toDatabase = 0, int timeoutMilliseconds = 0,
                MigrateOptions migrateOptions = MigrateOptions.None, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<RedisValue> DebugObjectAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<bool> GeoAddAsync(RedisKey key, double longitude, double latitude, RedisValue member, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<bool> GeoAddAsync(RedisKey key, GeoEntry value, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> GeoAddAsync(RedisKey key, GeoEntry[] values, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<bool> GeoRemoveAsync(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<double?> GeoDistanceAsync(RedisKey key, RedisValue member1, RedisValue member2, GeoUnit unit = GeoUnit.Meters,
                CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<string[]> GeoHashAsync(RedisKey key, RedisValue[] members, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<string> GeoHashAsync(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<GeoPosition?[]> GeoPositionAsync(RedisKey key, RedisValue[] members, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<GeoPosition?> GeoPositionAsync(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<GeoRadiusResult[]> GeoRadiusAsync(RedisKey key, RedisValue member, double radius, GeoUnit unit = GeoUnit.Meters, int count = -1,
                Order? order = null, GeoRadiusOptions options = GeoRadiusOptions.Default, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<GeoRadiusResult[]> GeoRadiusAsync(RedisKey key, double longitude, double latitude, double radius, GeoUnit unit = GeoUnit.Meters,
                int count = -1, Order? order = null, GeoRadiusOptions options = GeoRadiusOptions.Default, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> HashDecrementAsync(RedisKey key, RedisValue hashField, long value = 1, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<double> HashDecrementAsync(RedisKey key, RedisValue hashField, double value, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<bool> HashDeleteAsync(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> HashDeleteAsync(RedisKey key, RedisValue[] hashFields, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<bool> HashExistsAsync(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<RedisValue> HashGetAsync(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<Lease<byte>> HashGetLeaseAsync(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<RedisValue[]> HashGetAsync(RedisKey key, RedisValue[] hashFields, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<HashEntry[]> HashGetAllAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> HashIncrementAsync(RedisKey key, RedisValue hashField, long value = 1, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<double> HashIncrementAsync(RedisKey key, RedisValue hashField, double value, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<RedisValue[]> HashKeysAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> HashLengthAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task HashSetAsync(RedisKey key, HashEntry[] hashFields, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<bool> HashSetAsync(RedisKey key, RedisValue hashField, RedisValue value, When when = When.Always, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<RedisValue[]> HashValuesAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<bool> HyperLogLogAddAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<bool> HyperLogLogAddAsync(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> HyperLogLogLengthAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> HyperLogLogLengthAsync(RedisKey[] keys, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task HyperLogLogMergeAsync(RedisKey destination, RedisKey first, RedisKey second, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task HyperLogLogMergeAsync(RedisKey destination, RedisKey[] sourceKeys, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<EndPoint> IdentifyEndpointAsync(RedisKey key = new RedisKey(), CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<bool> KeyDeleteAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> KeyDeleteAsync(RedisKey[] keys, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<byte[]> KeyDumpAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<bool> KeyExistsAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> KeyExistsAsync(RedisKey[] keys, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<bool> KeyExpireAsync(RedisKey key, TimeSpan? expiry, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<bool> KeyExpireAsync(RedisKey key, DateTime? expiry, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<TimeSpan?> KeyIdleTimeAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<bool> KeyMoveAsync(RedisKey key, int database, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<bool> KeyPersistAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<RedisKey> KeyRandomAsync(CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<bool> KeyRenameAsync(RedisKey key, RedisKey newKey, When when = When.Always, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task KeyRestoreAsync(RedisKey key, byte[] value, TimeSpan? expiry = null, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<TimeSpan?> KeyTimeToLiveAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<RedisType> KeyTypeAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<RedisValue> ListGetByIndexAsync(RedisKey key, long index, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> ListInsertAfterAsync(RedisKey key, RedisValue pivot, RedisValue value, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> ListInsertBeforeAsync(RedisKey key, RedisValue pivot, RedisValue value, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<RedisValue> ListLeftPopAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> ListLeftPushAsync(RedisKey key, RedisValue value, When when = When.Always, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> ListLeftPushAsync(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> ListLengthAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<RedisValue[]> ListRangeAsync(RedisKey key, long start = 0, long stop = -1, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> ListRemoveAsync(RedisKey key, RedisValue value, long count = 0, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<RedisValue> ListRightPopAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<RedisValue> ListRightPopLeftPushAsync(RedisKey source, RedisKey destination, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> ListRightPushAsync(RedisKey key, RedisValue value, When when = When.Always, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> ListRightPushAsync(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task ListSetByIndexAsync(RedisKey key, long index, RedisValue value, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task ListTrimAsync(RedisKey key, long start, long stop, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<bool> LockExtendAsync(RedisKey key, RedisValue value, TimeSpan expiry, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<RedisValue> LockQueryAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<bool> LockReleaseAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<bool> LockTakeAsync(RedisKey key, RedisValue value, TimeSpan expiry, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> PublishAsync(RedisChannel channel, RedisValue message, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<RedisResult> ExecuteAsync(string command, params object[] args)
            {
                throw new NotImplementedException();
            }

            public Task<RedisResult> ExecuteAsync(string command, ICollection<object> args, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<RedisResult> ScriptEvaluateAsync(string script, RedisKey[] keys = null, RedisValue[] values = null, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<RedisResult> ScriptEvaluateAsync(byte[] hash, RedisKey[] keys = null, RedisValue[] values = null, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<RedisResult> ScriptEvaluateAsync(LuaScript script, object parameters = null, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<RedisResult> ScriptEvaluateAsync(LoadedLuaScript script, object parameters = null, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<bool> SetAddAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> SetAddAsync(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<RedisValue[]> SetCombineAsync(SetOperation operation, RedisKey first, RedisKey second, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<RedisValue[]> SetCombineAsync(SetOperation operation, RedisKey[] keys, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> SetCombineAndStoreAsync(SetOperation operation, RedisKey destination, RedisKey first, RedisKey second,
                CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> SetCombineAndStoreAsync(SetOperation operation, RedisKey destination, RedisKey[] keys, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<bool> SetContainsAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> SetLengthAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<RedisValue[]> SetMembersAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<bool> SetMoveAsync(RedisKey source, RedisKey destination, RedisValue value, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<RedisValue> SetPopAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<RedisValue[]> SetPopAsync(RedisKey key, long count, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<RedisValue> SetRandomMemberAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<RedisValue[]> SetRandomMembersAsync(RedisKey key, long count, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<bool> SetRemoveAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> SetRemoveAsync(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<RedisValue[]> SortAsync(RedisKey key, long skip = 0, long take = -1, Order order = Order.Ascending, SortType sortType = SortType.Numeric,
                RedisValue @by = new RedisValue(), RedisValue[] get = null, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> SortAndStoreAsync(RedisKey destination, RedisKey key, long skip = 0, long take = -1, Order order = Order.Ascending,
                SortType sortType = SortType.Numeric, RedisValue @by = new RedisValue(), RedisValue[] get = null, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<bool> SortedSetAddAsync(RedisKey key, RedisValue member, double score, CommandFlags flags)
            {
                throw new NotImplementedException();
            }

            public Task<bool> SortedSetAddAsync(RedisKey key, RedisValue member, double score, When when = When.Always, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> SortedSetAddAsync(RedisKey key, SortedSetEntry[] values, CommandFlags flags)
            {
                throw new NotImplementedException();
            }

            public Task<long> SortedSetAddAsync(RedisKey key, SortedSetEntry[] values, When when = When.Always, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> SortedSetCombineAndStoreAsync(SetOperation operation, RedisKey destination, RedisKey first, RedisKey second,
                Aggregate aggregate = Aggregate.Sum, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> SortedSetCombineAndStoreAsync(SetOperation operation, RedisKey destination, RedisKey[] keys,
                double[] weights = null, Aggregate aggregate = Aggregate.Sum, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<double> SortedSetDecrementAsync(RedisKey key, RedisValue member, double value, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<double> SortedSetIncrementAsync(RedisKey key, RedisValue member, double value, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> SortedSetLengthAsync(RedisKey key, double min = double.NegativeInfinity, double max = double.PositiveInfinity, Exclude exclude = Exclude.None,
                CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> SortedSetLengthByValueAsync(RedisKey key, RedisValue min, RedisValue max, Exclude exclude = Exclude.None,
                CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<RedisValue[]> SortedSetRangeByRankAsync(RedisKey key, long start = 0, long stop = -1, Order order = Order.Ascending,
                CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<SortedSetEntry[]> SortedSetRangeByRankWithScoresAsync(RedisKey key, long start = 0, long stop = -1, Order order = Order.Ascending,
                CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<RedisValue[]> SortedSetRangeByScoreAsync(RedisKey key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity, Exclude exclude = Exclude.None,
                Order order = Order.Ascending, long skip = 0, long take = -1, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<SortedSetEntry[]> SortedSetRangeByScoreWithScoresAsync(RedisKey key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity,
                Exclude exclude = Exclude.None, Order order = Order.Ascending, long skip = 0, long take = -1, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<RedisValue[]> SortedSetRangeByValueAsync(RedisKey key, RedisValue min, RedisValue max, Exclude exclude, long skip,
                long take = -1, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<RedisValue[]> SortedSetRangeByValueAsync(RedisKey key, RedisValue min = new RedisValue(), RedisValue max = new RedisValue(),
                Exclude exclude = Exclude.None, Order order = Order.Ascending, long skip = 0, long take = -1, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long?> SortedSetRankAsync(RedisKey key, RedisValue member, Order order = Order.Ascending, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<bool> SortedSetRemoveAsync(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> SortedSetRemoveAsync(RedisKey key, RedisValue[] members, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> SortedSetRemoveRangeByRankAsync(RedisKey key, long start, long stop, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> SortedSetRemoveRangeByScoreAsync(RedisKey key, double start, double stop, Exclude exclude = Exclude.None,
                CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> SortedSetRemoveRangeByValueAsync(RedisKey key, RedisValue min, RedisValue max, Exclude exclude = Exclude.None,
                CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<double?> SortedSetScoreAsync(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<SortedSetEntry?> SortedSetPopAsync(RedisKey key, Order order = Order.Ascending, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<SortedSetEntry[]> SortedSetPopAsync(RedisKey key, long count, Order order = Order.Ascending, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> StreamAcknowledgeAsync(RedisKey key, RedisValue groupName, RedisValue messageId, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> StreamAcknowledgeAsync(RedisKey key, RedisValue groupName, RedisValue[] messageIds, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<RedisValue> StreamAddAsync(RedisKey key, RedisValue streamField, RedisValue streamValue, RedisValue? messageId = null,
                int? maxLength = null, bool useApproximateMaxLength = false, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<RedisValue> StreamAddAsync(RedisKey key, NameValueEntry[] streamPairs, RedisValue? messageId = null, int? maxLength = null,
                bool useApproximateMaxLength = false, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<StreamEntry[]> StreamClaimAsync(RedisKey key, RedisValue consumerGroup, RedisValue claimingConsumer, long minIdleTimeInMs,
                RedisValue[] messageIds, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<RedisValue[]> StreamClaimIdsOnlyAsync(RedisKey key, RedisValue consumerGroup, RedisValue claimingConsumer, long minIdleTimeInMs,
                RedisValue[] messageIds, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<bool> StreamConsumerGroupSetPositionAsync(RedisKey key, RedisValue groupName, RedisValue position,
                CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<StreamConsumerInfo[]> StreamConsumerInfoAsync(RedisKey key, RedisValue groupName, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<bool> StreamCreateConsumerGroupAsync(RedisKey key, RedisValue groupName, RedisValue? position = null,
                CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> StreamDeleteAsync(RedisKey key, RedisValue[] messageIds, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> StreamDeleteConsumerAsync(RedisKey key, RedisValue groupName, RedisValue consumerName, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<bool> StreamDeleteConsumerGroupAsync(RedisKey key, RedisValue groupName, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<StreamGroupInfo[]> StreamGroupInfoAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<StreamInfo> StreamInfoAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> StreamLengthAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<StreamPendingInfo> StreamPendingAsync(RedisKey key, RedisValue groupName, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<StreamPendingMessageInfo[]> StreamPendingMessagesAsync(RedisKey key, RedisValue groupName, int count, RedisValue consumerName,
                RedisValue? minId = null, RedisValue? maxId = null, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<StreamEntry[]> StreamRangeAsync(RedisKey key, RedisValue? minId = null, RedisValue? maxId = null, int? count = null,
                Order messageOrder = Order.Ascending, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<StreamEntry[]> StreamReadAsync(RedisKey key, RedisValue position, int? count = null, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<RedisStream[]> StreamReadAsync(StreamPosition[] streamPositions, int? countPerStream = null, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<StreamEntry[]> StreamReadGroupAsync(RedisKey key, RedisValue groupName, RedisValue consumerName, RedisValue? position = null,
                int? count = null, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<RedisStream[]> StreamReadGroupAsync(StreamPosition[] streamPositions, RedisValue groupName, RedisValue consumerName,
                int? countPerStream = null, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> StreamTrimAsync(RedisKey key, int maxLength, bool useApproximateMaxLength = false, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> StringAppendAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> StringBitCountAsync(RedisKey key, long start = 0, long end = -1, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> StringBitOperationAsync(Bitwise operation, RedisKey destination, RedisKey first, RedisKey second = new RedisKey(),
                CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> StringBitOperationAsync(Bitwise operation, RedisKey destination, RedisKey[] keys, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> StringBitPositionAsync(RedisKey key, bool bit, long start = 0, long end = -1, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> StringDecrementAsync(RedisKey key, long value = 1, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<double> StringDecrementAsync(RedisKey key, double value, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<RedisValue> StringGetAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<RedisValue[]> StringGetAsync(RedisKey[] keys, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<Lease<byte>> StringGetLeaseAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<bool> StringGetBitAsync(RedisKey key, long offset, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<RedisValue> StringGetRangeAsync(RedisKey key, long start, long end, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<RedisValue> StringGetSetAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<RedisValueWithExpiry> StringGetWithExpiryAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> StringIncrementAsync(RedisKey key, long value = 1, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<double> StringIncrementAsync(RedisKey key, double value, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<long> StringLengthAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<bool> StringSetAsync(RedisKey key, RedisValue value, TimeSpan? expiry = null, When when = When.Always,
                CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<bool> StringSetAsync(KeyValuePair<RedisKey, RedisValue>[] values, When when = When.Always, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<bool> StringSetBitAsync(RedisKey key, long offset, bool bit, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Task<RedisValue> StringSetRangeAsync(RedisKey key, long offset, RedisValue value, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public IBatch CreateBatch(object asyncState = null)
            {
                throw new NotImplementedException();
            }

            public ITransaction CreateTransaction(object asyncState = null)
            {
                throw new NotImplementedException();
            }

            public void KeyMigrate(RedisKey key, EndPoint toServer, int toDatabase = 0, int timeoutMilliseconds = 0,
                MigrateOptions migrateOptions = MigrateOptions.None, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public RedisValue DebugObject(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public bool GeoAdd(RedisKey key, double longitude, double latitude, RedisValue member, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public bool GeoAdd(RedisKey key, GeoEntry value, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public long GeoAdd(RedisKey key, GeoEntry[] values, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public bool GeoRemove(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public double? GeoDistance(RedisKey key, RedisValue member1, RedisValue member2, GeoUnit unit = GeoUnit.Meters,
                CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public string[] GeoHash(RedisKey key, RedisValue[] members, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public string GeoHash(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public GeoPosition?[] GeoPosition(RedisKey key, RedisValue[] members, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public GeoPosition? GeoPosition(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public GeoRadiusResult[] GeoRadius(RedisKey key, RedisValue member, double radius, GeoUnit unit = GeoUnit.Meters, int count = -1,
                Order? order = null, GeoRadiusOptions options = GeoRadiusOptions.Default, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public GeoRadiusResult[] GeoRadius(RedisKey key, double longitude, double latitude, double radius, GeoUnit unit = GeoUnit.Meters,
                int count = -1, Order? order = null, GeoRadiusOptions options = GeoRadiusOptions.Default, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }
            
            public Lease<byte> HashGetLease(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<HashEntry> HashScan(RedisKey key, RedisValue pattern, int pageSize, CommandFlags flags)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<HashEntry> HashScan(RedisKey key, RedisValue pattern = new RedisValue(), int pageSize = 10, long cursor = 0,
                int pageOffset = 0, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }
            
            public bool HyperLogLogAdd(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public bool HyperLogLogAdd(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public long HyperLogLogLength(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public long HyperLogLogLength(RedisKey[] keys, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public void HyperLogLogMerge(RedisKey destination, RedisKey first, RedisKey second, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public void HyperLogLogMerge(RedisKey destination, RedisKey[] sourceKeys, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public EndPoint IdentifyEndpoint(RedisKey key = new RedisKey(), CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public bool KeyDelete(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public long KeyDelete(RedisKey[] keys, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public byte[] KeyDump(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public bool KeyExists(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public long KeyExists(RedisKey[] keys, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public bool KeyExpire(RedisKey key, TimeSpan? expiry, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public bool KeyExpire(RedisKey key, DateTime? expiry, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public TimeSpan? KeyIdleTime(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public bool KeyMove(RedisKey key, int database, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public bool KeyPersist(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public RedisKey KeyRandom(CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public bool KeyRename(RedisKey key, RedisKey newKey, When when = When.Always, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public void KeyRestore(RedisKey key, byte[] value, TimeSpan? expiry = null, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public TimeSpan? KeyTimeToLive(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public RedisType KeyType(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }
            

            public bool LockExtend(RedisKey key, RedisValue value, TimeSpan expiry, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public RedisValue LockQuery(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public bool LockRelease(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public bool LockTake(RedisKey key, RedisValue value, TimeSpan expiry, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public long Publish(RedisChannel channel, RedisValue message, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public RedisResult Execute(string command, params object[] args)
            {
                throw new NotImplementedException();
            }

            public RedisResult Execute(string command, ICollection<object> args, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public RedisResult ScriptEvaluate(string script, RedisKey[] keys = null, RedisValue[] values = null,
                CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public RedisResult ScriptEvaluate(byte[] hash, RedisKey[] keys = null, RedisValue[] values = null, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public RedisResult ScriptEvaluate(LuaScript script, object parameters = null, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public RedisResult ScriptEvaluate(LoadedLuaScript script, object parameters = null, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            

            public IEnumerable<RedisValue> SetScan(RedisKey key, RedisValue pattern, int pageSize, CommandFlags flags)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<RedisValue> SetScan(RedisKey key, RedisValue pattern = new RedisValue(), int pageSize = 10, long cursor = 0,
                int pageOffset = 0, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public RedisValue[] Sort(RedisKey key, long skip = 0, long take = -1, Order order = Order.Ascending, SortType sortType = SortType.Numeric,
                RedisValue @by = new RedisValue(), RedisValue[] get = null, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public long SortAndStore(RedisKey destination, RedisKey key, long skip = 0, long take = -1, Order order = Order.Ascending,
                SortType sortType = SortType.Numeric, RedisValue @by = new RedisValue(), RedisValue[] get = null, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            

            public long StreamAcknowledge(RedisKey key, RedisValue groupName, RedisValue messageId, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public long StreamAcknowledge(RedisKey key, RedisValue groupName, RedisValue[] messageIds, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public RedisValue StreamAdd(RedisKey key, RedisValue streamField, RedisValue streamValue, RedisValue? messageId = null,
                int? maxLength = null, bool useApproximateMaxLength = false, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public RedisValue StreamAdd(RedisKey key, NameValueEntry[] streamPairs, RedisValue? messageId = null, int? maxLength = null,
                bool useApproximateMaxLength = false, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public StreamEntry[] StreamClaim(RedisKey key, RedisValue consumerGroup, RedisValue claimingConsumer, long minIdleTimeInMs,
                RedisValue[] messageIds, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public RedisValue[] StreamClaimIdsOnly(RedisKey key, RedisValue consumerGroup, RedisValue claimingConsumer,
                long minIdleTimeInMs, RedisValue[] messageIds, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public bool StreamConsumerGroupSetPosition(RedisKey key, RedisValue groupName, RedisValue position, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public StreamConsumerInfo[] StreamConsumerInfo(RedisKey key, RedisValue groupName, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public bool StreamCreateConsumerGroup(RedisKey key, RedisValue groupName, RedisValue? position = null,
                CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public long StreamDelete(RedisKey key, RedisValue[] messageIds, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public long StreamDeleteConsumer(RedisKey key, RedisValue groupName, RedisValue consumerName, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public bool StreamDeleteConsumerGroup(RedisKey key, RedisValue groupName, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public StreamGroupInfo[] StreamGroupInfo(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public StreamInfo StreamInfo(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public long StreamLength(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public StreamPendingInfo StreamPending(RedisKey key, RedisValue groupName, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public StreamPendingMessageInfo[] StreamPendingMessages(RedisKey key, RedisValue groupName, int count, RedisValue consumerName,
                RedisValue? minId = null, RedisValue? maxId = null, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public StreamEntry[] StreamRange(RedisKey key, RedisValue? minId = null, RedisValue? maxId = null, int? count = null,
                Order messageOrder = Order.Ascending, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public StreamEntry[] StreamRead(RedisKey key, RedisValue position, int? count = null, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public RedisStream[] StreamRead(StreamPosition[] streamPositions, int? countPerStream = null, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public StreamEntry[] StreamReadGroup(RedisKey key, RedisValue groupName, RedisValue consumerName, RedisValue? position = null,
                int? count = null, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public RedisStream[] StreamReadGroup(StreamPosition[] streamPositions, RedisValue groupName, RedisValue consumerName,
                int? countPerStream = null, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public long StreamTrim(RedisKey key, int maxLength, bool useApproximateMaxLength = false, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public long StringAppend(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public long StringBitCount(RedisKey key, long start = 0, long end = -1, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public long StringBitOperation(Bitwise operation, RedisKey destination, RedisKey first, RedisKey second = new RedisKey(),
                CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public long StringBitOperation(Bitwise operation, RedisKey destination, RedisKey[] keys, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public long StringBitPosition(RedisKey key, bool bit, long start = 0, long end = -1, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public bool StringGetBit(RedisKey key, long offset, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }
            
            public bool StringSetBit(RedisKey key, long offset, bool bit, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public Lease<byte> StringGetLease(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public RedisValueWithExpiry StringGetWithExpiry(RedisKey key, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }
            
            
            
            public IEnumerable<SortedSetEntry> SortedSetScan(RedisKey key, RedisValue pattern, int pageSize, CommandFlags flags)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<SortedSetEntry> SortedSetScan(RedisKey key, RedisValue pattern = new RedisValue(), int pageSize = 10, long cursor = 0,
                int pageOffset = 0, CommandFlags flags = CommandFlags.None)
            {
                throw new NotImplementedException();
            }

            public int Database { get; }
            
            #endregion
        }
        
        public static Dictionary<Type, Type> GetMapToProxy()
        {
            return new Dictionary<Type, Type>()
            {
                { typeof(IDatabase), typeof(DatabaseProxy) }
            };
        }
    }
}