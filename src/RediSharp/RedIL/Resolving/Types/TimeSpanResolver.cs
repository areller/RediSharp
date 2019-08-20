using System;
using System.Linq;
using Humanizer;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Nodes;
using RediSharp.RedIL.Resolving.Attributes;

namespace RediSharp.RedIL.Resolving.Types
{
    class TimeSpanResolverPack
    {
        class ConstructorResolver : RedILObjectResolver
        {
            public override ExpressionNode Resolve(Context context, ExpressionNode[] arguments, ExpressionNode[] elements)
            {
                return null;
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
            
            [RedILResolve(typeof(ValueResolver), (double) 1 / 86400)]
            public double TotalDays { get; }

            [RedILResolve(typeof(ValueResolver), (double) 1 / 3600)]
            public double TotalHours { get; }

            [RedILResolve(typeof(ValueResolver), 1000)]
            public double TotalMilliseconds { get; }

            [RedILResolve(typeof(ValueResolver), (double) 1 / 60)]
            public double TotalMinutes { get; }

            [RedILResolve(typeof(ValueResolver), 1)]
            public double TotalSeconds { get; }

            [RedILResolve(typeof(FromMethodResolver), 86400)]
            public static TimeSpan FromDays(double value) => TimeSpan.FromDays(value);

            [RedILResolve(typeof(FromMethodResolver), 3600)]
            public static TimeSpan FromHours(double value) => TimeSpan.FromHours(value);

            [RedILResolve(typeof(FromMethodResolver), (double) 1 / 1000)]
            public static TimeSpan FromMilliseconds(double value) => TimeSpan.FromMilliseconds(value);

            [RedILResolve(typeof(FromMethodResolver), 60)]
            public static TimeSpan FromMinutes(double value) => TimeSpan.FromMinutes(value);

            [RedILResolve(typeof(FromMethodResolver), 1)]
            public static TimeSpan FromSeconds(double value) => TimeSpan.FromSeconds(value);
        }

        public class TimeSpanResolver : TypeResolver<TimeSpan>
        {
            public TimeSpanResolver()
            {
                Proxy<TimeSpanProxy>();
            }
        }
    }
}