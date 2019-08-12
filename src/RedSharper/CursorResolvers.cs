using System;
using System.Linq;
using RedSharper.Enums;
using RedSharper.RedIL;
using RedSharper.RedIL.Attributes;

namespace RedSharper
{
    class CursorRedisMethodResolver : RedILResolver
    {
        private RedisCommand _cmd;
        
        public CursorRedisMethodResolver(object arg)
        {
            _cmd = (RedisCommand) arg;
        }
        
        public override ExpressionNode Resolve(IExpressionVisitor visitor, ExpressionNode caller, ExpressionNode[] arguments)
        {
            return new CallRedisMethodNode(_cmd, arguments.Skip(1).ToArray());
        }
    }
}