using System;
using System.Collections.Generic;
using System.Linq;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Nodes;
using RediSharp.RedIL.Resolving;
using RediSharp.RedIL.Resolving.Attributes;

namespace RediSharp.Lib.Internal.Types
{
    class TimeSpanResolverPack
    {
        class ConstructorResolver : RedILObjectResolver
        {
            public override ExpressionNode Resolve(Context context, ExpressionNode[] arguments,
                ExpressionNode[] elements)
            {
                switch (arguments.Length)
                {
                    case 3:
                        return (ConstantValueNode) 3600000 * arguments[0] + (ConstantValueNode) 60000 * arguments[1] +
                               (ConstantValueNode) 1000 * arguments[2];
                    case 4:
                        return (ConstantValueNode) 86400000 * arguments[0] +
                               (ConstantValueNode) 3600000 * arguments[1] +
                               (ConstantValueNode) 60000 * arguments[2] + (ConstantValueNode) 1000 * arguments[3];
                    case 5:
                        return (ConstantValueNode) 86400000 * arguments[0] +
                               (ConstantValueNode) 3600000 * arguments[1] +
                               (ConstantValueNode) 60000 * arguments[2] + (ConstantValueNode) 1000 * arguments[3] +
                               arguments[4];
                    default:
                        return null;
                }
            }
        }

        class FromMethodResolver : RedILMethodResolver
        {
            private double _factor;

            public FromMethodResolver(object factorArg)
            {
                _factor = (double) factorArg;
            }

            public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
            {
                var arg = arguments.First();
                if (_factor == 1)
                {
                    return arg;
                }

                return BinaryExpressionNode.Create(BinaryExpressionOperator.Multiply,
                    new ConstantValueNode(DataValueType.Float, _factor), arg);
            }
        }

        class ValueResolver : RedILMemberResolver
        {
            private double _factor;

            public ValueResolver(object factorArg)
            {
                _factor = (double) factorArg;
            }

            public override ExpressionNode Resolve(Context context, ExpressionNode caller)
            {
                if (_factor == 1)
                {
                    return caller;
                }

                return BinaryExpressionNode.Create(BinaryExpressionOperator.Multiply,
                    new ConstantValueNode(DataValueType.Float, _factor), caller);
            }
        }

        [RedILDataType(DataValueType.Integer)]
        class TimeSpanProxy
        {
            [RedILResolve(typeof(ConstructorResolver))]
            public TimeSpanProxy(int hours, int minutes, int seconds)
            {
            }

            [RedILResolve(typeof(ConstructorResolver))]
            public TimeSpanProxy(int days, int hours, int minutes, int seconds)
            {
            }

            [RedILResolve(typeof(ConstructorResolver))]
            public TimeSpanProxy(int days, int hours, int minutes, int seconds, int milliseconds)
            {
            }

            [RedILResolve(typeof(ValueResolver), (double) 1 / 86400000)]
            public double TotalDays { get; }

            [RedILResolve(typeof(ValueResolver), (double) 1 / 3600000)]
            public double TotalHours { get; }

            [RedILResolve(typeof(ValueResolver), (double)1)]
            public double TotalMilliseconds { get; }

            [RedILResolve(typeof(ValueResolver), (double) 1 / 60000)]
            public double TotalMinutes { get; }

            [RedILResolve(typeof(ValueResolver), (double) 1/1000)]
            public double TotalSeconds { get; }

            [RedILResolve(typeof(FromMethodResolver), (double) 86400000)]
            public static TimeSpan FromDays(double value) => TimeSpan.FromDays(value);

            [RedILResolve(typeof(FromMethodResolver), (double) 3600000)]
            public static TimeSpan FromHours(double value) => TimeSpan.FromHours(value);

            [RedILResolve(typeof(FromMethodResolver), (double) 1)]
            public static TimeSpan FromMilliseconds(double value) => TimeSpan.FromMilliseconds(value);

            [RedILResolve(typeof(FromMethodResolver), (double) 60000)]
            public static TimeSpan FromMinutes(double value) => TimeSpan.FromMinutes(value);

            [RedILResolve(typeof(FromMethodResolver), (double) 1000)]
            public static TimeSpan FromSeconds(double value) => TimeSpan.FromSeconds(value);
        }

        public static Dictionary<Type, Type> GetMapToProxy()
        {
            return new Dictionary<Type, Type>()
            {
                {typeof(TimeSpan), typeof(TimeSpanProxy)}
            };
        }
    }
}